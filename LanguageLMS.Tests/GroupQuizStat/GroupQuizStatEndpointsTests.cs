using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.GroupQuizStat;

[Collection("api")]
public class GroupQuizStatEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<long> SubjectAsync()
        => (await Json(await _admin.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("Subj")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();

    private async Task<long> QuestionAsync(long subjectId, string correct)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent(Unique("Q")), "title" },
            { new StringContent("a"), "variant_a" }, { new StringContent("b"), "variant_b" },
            { new StringContent("c"), "variant_c" }, { new StringContent("d"), "variant_d" },
            { new StringContent(correct), "correct_answer" }
        };
        return (await Json(await _admin.PostAsync("/api/question", form))).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> LessonAsync(long subjectId)
        => (await Json(await _admin.PostAsync("/api/lesson", new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" }, { new StringContent(Unique("L")), "title" }
        }))).GetProperty("data").GetProperty("id").GetInt64();

    private async Task<long> QuizAsync(long lessonId)
        => (await Json(await _admin.PostAsync("/api/quiz", new MultipartFormDataContent
        {
            { new StringContent(lessonId.ToString()), "lesson_id" }, { new StringContent(Unique("Quiz")), "title" }
        }))).GetProperty("data").GetProperty("id").GetInt64();

    [Fact]
    public async Task Group_quiz_stat_aggregates_student_results()
    {
        var subjectId = await SubjectAsync();
        var q1 = await QuestionAsync(subjectId, "A");
        var q2 = await QuestionAsync(subjectId, "B");
        var lessonId = await LessonAsync(subjectId);
        var quizId = await QuizAsync(lessonId);
        await _admin.PostAsJsonAsync($"/api/quiz/{quizId}/questions", new { questionIds = new[] { q1, q2 } });

        var groupId = (await Json(await _admin.PostAsync("/api/group", new MultipartFormDataContent { { new StringContent(Unique("G")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { subjectId } });

        var reg = await _admin.PostAsJsonAsync("/api/auth/register", new { fullName = "Stat Student", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var d = (await Json(reg)).GetProperty("data");
        var studentId = d.GetProperty("user").GetProperty("id").GetInt64();
        var studentToken = d.GetProperty("token").GetString()!;
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { studentId } });

        var student = factory.CreateClient();
        student.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);

        var start = await Json(await student.PostAsJsonAsync("/api/student-quiz/start", new { quizId }));
        var attemptId = start.GetProperty("data").GetProperty("attemptId").GetInt64();
        await student.PostAsJsonAsync("/api/student-quiz/complete", new
        {
            attemptId,
            answers = new object[] { new { questionId = q1, answer = "A" }, new { questionId = q2, answer = "C" } }
        });

        var stat = await Json(await _admin.GetAsync($"/api/group/{groupId}/quiz-stat"));
        var data = stat.GetProperty("data");
        Assert.Equal(1, data.GetProperty("quizCount").GetInt32());
        Assert.Equal(2, data.GetProperty("totalQuestions").GetInt32());

        var row = data.GetProperty("students").EnumerateArray().First(x => x.GetProperty("studentId").GetInt64() == studentId);
        Assert.Equal(1, row.GetProperty("attemptedQuizCount").GetInt32());
        Assert.Equal(1, row.GetProperty("correct").GetInt32());
        Assert.Equal(1, row.GetProperty("wrong").GetInt32());
        Assert.Equal(50, row.GetProperty("scorePercent").GetDouble());
    }

    [Fact]
    public async Task Quiz_stat_requires_teacher()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/group/1/quiz-stat")).StatusCode);
}
