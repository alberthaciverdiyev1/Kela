using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kela.Tests.StudentQuiz;

[Collection("api")]
public class StudentQuizEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<(long id, string token)> RegisterStudentAsync()
    {
        var r = await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "S", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var d = (await Json(r)).GetProperty("data");
        return (d.GetProperty("user").GetProperty("id").GetInt64(), d.GetProperty("token").GetString()!);
    }

    private HttpClient Authed(string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    [Fact]
    public async Task Start_complete_and_read_results()
    {
        var (studentId, token) = await RegisterStudentAsync();
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subj"));
        var q1 = await TestData.QuestionAsync(factory, subjectId, Unique("Q"), "A");
        var q2 = await TestData.QuestionAsync(factory, subjectId, Unique("Q"), "B");
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("L"));
        var quizId = await TestData.QuizAsync(factory, lessonId, Unique("Quiz"));
        await TestData.AssignQuestionsAsync(factory, quizId, q1, q2);

        var groupId = await TestData.GroupAsync(factory, Unique("G"));
        await TestData.AssignSubjectsAsync(factory, groupId, subjectId);
        await TestData.AssignStudentsAsync(factory, groupId, new[] { studentId });

        var student = Authed(token);

        var start = await student.PostAsJsonAsync("/api/student-quiz/start", new { quizId });
        Assert.Equal(HttpStatusCode.OK, start.StatusCode);
        var startData = (await Json(start)).GetProperty("data");
        var attemptId = startData.GetProperty("attemptId").GetInt64();
        var questions = startData.GetProperty("questions").EnumerateArray().ToList();
        Assert.Equal(2, questions.Count);
        // correct answer sızdırılmamalı
        Assert.False(questions[0].TryGetProperty("correctAnswer", out _));

        var complete = await student.PostAsJsonAsync("/api/student-quiz/complete", new
        {
            attemptId,
            answers = new object[]
            {
                new { questionId = q1, answer = "A" }, // doğru
                new { questionId = q2, answer = "C" }  // yanlış
            }
        });
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
        var result = (await Json(complete)).GetProperty("data");
        Assert.Equal(1, result.GetProperty("correctCount").GetInt32());
        Assert.Equal(50, result.GetProperty("scorePercent").GetInt32());

        var list = await student.GetAsync("/api/student-quiz/results");
        Assert.Equal(1, (await Json(list)).GetProperty("data").GetArrayLength());

        var detail = await student.GetAsync($"/api/student-quiz/results/{attemptId}");
        var items = (await Json(detail)).GetProperty("data").GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(2, items.Count);
        Assert.Contains(items, x => x.GetProperty("isRight").GetBoolean());
    }

    [Fact]
    public async Task Start_without_access_returns_forbidden()
    {
        var (_, token) = await RegisterStudentAsync();
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subj"));
        await TestData.QuestionAsync(factory, subjectId, Unique("Q"), "A");
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("L"));
        var quizId = await TestData.QuizAsync(factory, lessonId, Unique("Quiz"));

        var student = Authed(token);
        var response = await student.PostAsJsonAsync("/api/student-quiz/start", new { quizId });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Results_filtered_by_date_range()
    {
        var student = await CompletedAttemptAsync();
        var now = DateTimeOffset.UtcNow;

        var inside = $"/api/student-quiz/results?from={Uri.EscapeDataString(now.AddHours(-1).ToString("o"))}&to={Uri.EscapeDataString(now.AddHours(1).ToString("o"))}";
        Assert.Equal(1, (await Json(await student.GetAsync(inside))).GetProperty("data").GetArrayLength());

        var outside = $"/api/student-quiz/results?from={Uri.EscapeDataString(now.AddDays(-10).ToString("o"))}&to={Uri.EscapeDataString(now.AddDays(-9).ToString("o"))}";
        Assert.Equal(0, (await Json(await student.GetAsync(outside))).GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task Results_filtered_with_non_utc_offset()
    {
        var student = await CompletedAttemptAsync();
        // İstemci yerel saat dilimini (məs. +04:00) göndərir — Postgres timestamptz yalnız UTC qəbul edir.
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(4));

        var url = $"/api/student-quiz/results?from={Uri.EscapeDataString(now.AddHours(-1).ToString("o"))}&to={Uri.EscapeDataString(now.AddHours(1).ToString("o"))}";
        var response = await student.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, (await Json(response)).GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task Requires_auth()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/student-quiz/results")).StatusCode);

    private async Task<HttpClient> CompletedAttemptAsync()
    {
        var (studentId, token) = await RegisterStudentAsync();
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subj"));
        var q1 = await TestData.QuestionAsync(factory, subjectId, Unique("Q"), "A");
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("L"));
        var quizId = await TestData.QuizAsync(factory, lessonId, Unique("Quiz"));
        await TestData.AssignQuestionsAsync(factory, quizId, q1);

        var groupId = await TestData.GroupAsync(factory, Unique("G"));
        await TestData.AssignSubjectsAsync(factory, groupId, subjectId);
        await TestData.AssignStudentsAsync(factory, groupId, new[] { studentId });

        var student = Authed(token);
        var start = await student.PostAsJsonAsync("/api/student-quiz/start", new { quizId });
        var attemptId = (await Json(start)).GetProperty("data").GetProperty("attemptId").GetInt64();

        var complete = await student.PostAsJsonAsync("/api/student-quiz/complete", new
        {
            attemptId,
            answers = new object[] { new { questionId = q1, answer = "A" } }
        });
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
        return student;
    }
}
