using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Edutor.Core.Common.Exceptions;
using Edutor.Core.Data;
using Edutor.Core.Modules.Auth.Abstractions;
using Edutor.Core.Modules.Auth.Requests;
using Edutor.Core.Modules.Group.Abstractions;
using Edutor.Core.Modules.Group.Requests;
using Edutor.Core.Modules.Homework.Abstractions;
using Edutor.Core.Modules.Homework.Requests;
using Edutor.Core.Modules.Quiz.Abstractions;
using Edutor.Core.Modules.Subject.Abstractions;
using Edutor.Core.Modules.Subject.Requests;
using Edutor.Core.Modules.Question.Abstractions;
using Edutor.Core.Modules.Question.Requests;
using StudentQuizEntity = Edutor.Core.Modules.Quiz.Entities.StudentQuiz;
using StudentQuizQuestionEntity = Edutor.Core.Modules.Quiz.Entities.StudentQuizQuestion;
using Edutor.Core.Modules.Quiz.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Tests.Homework;

[Collection("api")]
public class HomeworkTests(ApiFactory factory)
{

    private async Task<T> With<TService, T>(Func<TService, Task<T>> action) where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private async Task<long> CreateTeacherAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new Core.Modules.User.Entities.User
        {
            Name = name,
            Email = $"{Guid.NewGuid():N}@test.local",
            PasswordHash = "x",
            Type = Core.Modules.User.Entities.UserTypes.Teacher
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    private async Task<long> RegisterStudentAsync(string name)
    {
        var response = await With<IAuthService, Core.Modules.Auth.Responses.AuthResponse>(s => s.RegisterAsync(new RegisterRequest
        {
            FullName = name,
            Email = $"{Guid.NewGuid():N}@test.local",
            Password = "Password1"
        }));

        return response.User.Id;
    }

    private async Task<long> CreateGroupAsync(long ownerId, params long[] studentIds)
    {
        using var scope = factory.Services.CreateScope();
        var groups = scope.ServiceProvider.GetRequiredService<IGroupService>();

        var group = await groups.CreateAsync(new GroupUpsertRequest
        {
            Name = $"G-{Guid.NewGuid():N}",
            UserId = ownerId
        });

        if (studentIds.Length > 0)
        {
            await groups.AssignStudentsAsync(group.Id, new AssignStudentsRequest
            {
                StudentIds = studentIds.ToList()
            }, ownerId);
        }

        return group.Id;
    }

    private static HomeworkUpsertRequest Request(long groupId, long? quizId = null) => new()
    {
        Title = $"HW-{Guid.NewGuid():N}",
        Description = "Test homework",
        GroupIds = [groupId],
        GroupDueDates = [DateTimeOffset.UtcNow.AddDays(1)],
        GroupScores = [100],
        QuizIds = quizId is long id ? [id] : []
    };

    [Fact]
    public async Task Publishing_snapshots_distinct_students_once()
    {
        var teacherA = await CreateTeacherAsync("Teacher A");
        var s1 = await RegisterStudentAsync("Student One");
        var s2 = await RegisterStudentAsync("Student Two");
        var g1 = await CreateGroupAsync(teacherA, s1, s2);
        var g2 = await CreateGroupAsync(teacherA, s2);

        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IHomeworkService>();

        var draft = await service.CreateAsync(new HomeworkUpsertRequest
        {
            Title = "Snapshot",
            GroupIds = [g1, g2],
            GroupDueDates = [DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(2)],
            GroupScores = [100, 80]
        }, teacherA);

        Assert.False(draft.IsPublished);

        var published = await service.PublishAsync(draft.Id, teacherA);
        Assert.True(published.IsPublished);
        Assert.Equal(2, published.AssignedCount);

        var again = await service.PublishAsync(draft.Id, teacherA);
        Assert.Equal(2, again.AssignedCount);
    }

    [Fact]
    public async Task Each_group_keeps_its_own_deadline_and_score()
    {
        var teacherA = await CreateTeacherAsync("Teacher A");
        var earlyStudent = await RegisterStudentAsync("Early Student");
        var lateStudent = await RegisterStudentAsync("Late Student");
        var earlyGroup = await CreateGroupAsync(teacherA, earlyStudent);
        var lateGroup = await CreateGroupAsync(teacherA, lateStudent);

        using var scope = factory.Services.CreateScope();
        var homeworkService = scope.ServiceProvider.GetRequiredService<IHomeworkService>();
        var studentService = scope.ServiceProvider.GetRequiredService<IStudentHomeworkService>();

        var homework = await homeworkService.CreateAsync(new HomeworkUpsertRequest
        {
            Title = "Per-group terms",
            GroupIds = [earlyGroup, lateGroup],
            GroupDueDates = [DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(5)],
            GroupScores = [40, 90]
        }, teacherA);

        await homeworkService.PublishAsync(homework.Id, teacherA);

        var early = await studentService.GetAsync(earlyStudent, homework.Id);
        var late = await studentService.GetAsync(lateStudent, homework.Id);

        Assert.Equal(40, early.MaxScore);
        Assert.Equal(90, late.MaxScore);

        var earlySubmit = await studentService.SubmitAsync(earlyStudent, homework.Id, new SubmitHomeworkRequest { Text = "early" });
        var lateSubmit = await studentService.SubmitAsync(lateStudent, homework.Id, new SubmitHomeworkRequest { Text = "late" });
        Assert.True(earlySubmit.IsLate);
        Assert.False(lateSubmit.IsLate);

        // Bal qrupun öz limiti ilə yoxlanılır.
        await Assert.ThrowsAsync<ArgumentException>(() => homeworkService.GradeAsync(
            homework.Id, earlyStudent, new GradeHomeworkRequest { Score = 50 }, teacherA, teacherA));

        var graded = await homeworkService.GradeAsync(
            homework.Id, earlyStudent, new GradeHomeworkRequest { Score = 40 }, teacherA, teacherA);
        Assert.Equal(40, graded.Score);
    }

    [Fact]
    public async Task Teacher_cannot_use_another_teachers_group_or_homework()
    {
        var teacherA = await CreateTeacherAsync("Teacher A");
        var teacherB = await CreateTeacherAsync("Teacher B");
        var student = await RegisterStudentAsync("Student");
        var foreignGroup = await CreateGroupAsync(teacherB, student);

        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IHomeworkService>();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.CreateAsync(Request(foreignGroup), teacherA));

        var ownGroup = await CreateGroupAsync(teacherA);
        var homework = await service.CreateAsync(Request(ownGroup), teacherA);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetAsync(homework.Id, teacherB));
    }

    [Fact]
    public async Task Published_homework_locks_groups_and_score()
    {
        var teacherA = await CreateTeacherAsync("Teacher A");
        var group = await CreateGroupAsync(teacherA);
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IHomeworkService>();

        var homework = await service.CreateAsync(Request(group), teacherA);
        await service.PublishAsync(homework.Id, teacherA);

        var update = Request(group);
        update.GroupScores = [50];

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(homework.Id, update, teacherA));
    }

    [Fact]
    public async Task Student_can_submit_and_teacher_can_grade_or_return()
    {
        var teacherA = await CreateTeacherAsync("Teacher A");
        var studentId = await RegisterStudentAsync("Submitter");
        var group = await CreateGroupAsync(teacherA, studentId);

        using var scope = factory.Services.CreateScope();
        var homeworkService = scope.ServiceProvider.GetRequiredService<IHomeworkService>();
        var studentService = scope.ServiceProvider.GetRequiredService<IStudentHomeworkService>();

        var request = Request(group);
        request.GroupDueDates = [DateTimeOffset.UtcNow.AddDays(-1)]; // gecikmiş
        var homework = await homeworkService.CreateAsync(request, teacherA);
        await homeworkService.PublishAsync(homework.Id, teacherA);

        var submitted = await studentService.SubmitAsync(studentId, homework.Id, new SubmitHomeworkRequest
        {
            Text = "My answer"
        });

        Assert.Equal("submitted", submitted.Status);
        Assert.True(submitted.IsLate);
        Assert.Equal(1, submitted.AttemptCount);

        // Qaytarılmış kimi qiymətləndir → yenidən təqdim mümkün.
        var returned = await homeworkService.GradeAsync(homework.Id, studentId, new GradeHomeworkRequest
        {
            Score = 80,
            Feedback = "Please fix",
            ReturnToStudent = true
        }, teacherA, teacherA);

        Assert.Equal("returned", returned.Status);

        var resubmitted = await studentService.SubmitAsync(studentId, homework.Id, new SubmitHomeworkRequest
        {
            Text = "Fixed"
        });
        Assert.Equal("submitted", resubmitted.Status);
        Assert.Equal(2, resubmitted.AttemptCount);

        var graded = await homeworkService.GradeAsync(homework.Id, studentId, new GradeHomeworkRequest
        {
            Score = 90,
            Feedback = "Great",
            ReturnToStudent = false
        }, teacherA, teacherA);

        Assert.Equal("graded", graded.Status);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            studentService.SubmitAsync(studentId, homework.Id, new SubmitHomeworkRequest { Text = "Again" }));
    }

    [Fact]
    public async Task Attached_quiz_result_is_read_from_the_latest_completed_attempt()
    {
        var teacherA = await CreateTeacherAsync("Teacher A");
        var studentId = await RegisterStudentAsync("Quiz Student");
        var group = await CreateGroupAsync(teacherA, studentId);

        var quiz = await With<IQuizService, Core.Modules.Quiz.Responses.QuizResponse>(s => s.CreateAsync(new QuizUpsertRequest
        {
            Title = $"Q-{Guid.NewGuid():N}",
            UserId = teacherA,
            DurationMinutes = 10
        }));

        using var scope = factory.Services.CreateScope();
        var homeworkService = scope.ServiceProvider.GetRequiredService<IHomeworkService>();
        var studentService = scope.ServiceProvider.GetRequiredService<IStudentHomeworkService>();

        var homework = await homeworkService.CreateAsync(Request(group, quiz.Id), teacherA);
        await homeworkService.PublishAsync(homework.Id, teacherA);

        using (var dbScope = factory.Services.CreateScope())
        {
            var db = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var subjects = dbScope.ServiceProvider.GetRequiredService<ISubjectService>();
            var subject = await subjects.CreateAsync(new SubjectUpsertRequest
            {
                Name = $"S-{Guid.NewGuid():N}",
                UserId = teacherA
            });

            var questionService = dbScope.ServiceProvider.GetRequiredService<IQuestionService>();
            var q1 = await questionService.CreateAsync(new QuestionUpsertRequest
            {
                SubjectId = subject.Id,
                Title = "Question 1",
                VariantA = "a", VariantB = "b", VariantC = "c", VariantD = "d",
                CorrectAnswer = "A", TitleType = "text", VariantType = "text"
            });
            var q2 = await questionService.CreateAsync(new QuestionUpsertRequest
            {
                SubjectId = subject.Id,
                Title = "Question 2",
                VariantA = "a", VariantB = "b", VariantC = "c", VariantD = "d",
                CorrectAnswer = "A", TitleType = "text", VariantType = "text"
            });

            var quizService = dbScope.ServiceProvider.GetRequiredService<IQuizService>();
            await quizService.AssignQuestionsAsync(quiz.Id, new AssignQuestionsRequest
            {
                QuestionIds = [q1.Id, q2.Id]
            }, teacherA);

            var attempt = new StudentQuizEntity
            {
                StudentId = studentId,
                QuizId = quiz.Id,
                AttemptTime = 1,
                TotalQuestionCount = 2,
                CompletedQuestionCount = 2,
                IsCompleted = true,
                EndDate = DateTimeOffset.UtcNow
            };

            db.StudentQuizzes.Add(attempt);
            await db.SaveChangesAsync();

            db.StudentQuizQuestions.AddRange(
                new StudentQuizQuestionEntity { StudentQuizId = attempt.Id, QuestionId = q1.Id, StudentAnswer = "A", IsRight = true },
                new StudentQuizQuestionEntity { StudentQuizId = attempt.Id, QuestionId = q2.Id, StudentAnswer = "B", IsRight = false });

            await db.SaveChangesAsync();
        }

        var detail = await studentService.GetAsync(studentId, homework.Id);

        var result = Assert.Single(detail.Quizzes);
        Assert.True(result.Completed);
        Assert.Equal(1, result.CorrectCount);
        Assert.Equal(2, result.QuestionCount);
        Assert.Equal(50, result.ScorePercent);
    }

    private HttpClient StudentClient(long studentId, string email)
    {
        using var scope = factory.Services.CreateScope();
        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens.CreateToken(studentId, email, "Student", "student", 0));
        return client;
    }

    private async Task<string> EmailOfAsync(long studentId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Students.Where(x => x.Id == studentId).Select(x => x.Email).FirstAsync();
    }

    [Fact]
    public async Task Student_endpoints_require_auth_and_only_return_assigned_homework()
    {
        var teacherA = await CreateTeacherAsync("Teacher A");
        var studentId = await RegisterStudentAsync("Endpoint Student");
        var otherId = await RegisterStudentAsync("Other Student");
        var group = await CreateGroupAsync(teacherA, studentId);

        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IHomeworkService>();
        var homework = await service.CreateAsync(Request(group), teacherA);
        await service.PublishAsync(homework.Id, teacherA);

        var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/student-homework")).StatusCode);

        var client = StudentClient(studentId, await EmailOfAsync(studentId));

        var list = await client.GetAsync("/api/student-homework");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using (var document = JsonDocument.Parse(await list.Content.ReadAsStringAsync()))
        {
            Assert.Equal(1, document.RootElement.GetProperty("data").GetArrayLength());
        }

        var detail = await client.GetAsync($"/api/student-homework/{homework.Id}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);

        using var form = new MultipartFormDataContent { { new StringContent("Endpoint answer"), "text" } };
        var submit = await client.PostAsync($"/api/student-homework/{homework.Id}/submission", form);
        Assert.Equal(HttpStatusCode.OK, submit.StatusCode);
        using (var document = JsonDocument.Parse(await submit.Content.ReadAsStringAsync()))
        {
            Assert.Equal("submitted", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        }

        var other = StudentClient(otherId, await EmailOfAsync(otherId));
        var forbidden = await other.GetAsync($"/api/student-homework/{homework.Id}");
        Assert.Equal(HttpStatusCode.NotFound, forbidden.StatusCode);
    }
}
