using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Quiz;

[Collection("api")]
public class QuizRepeatEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<long> SubjectAsync()
        => (await Json(await _admin.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("S")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();

    private async Task<long> QuestionAsync(long subjectId)
        => (await Json(await _admin.PostAsync("/api/question", new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" }, { new StringContent(Unique("Q")), "title" },
            { new StringContent("a"), "variant_a" }, { new StringContent("b"), "variant_b" },
            { new StringContent("c"), "variant_c" }, { new StringContent("d"), "variant_d" },
            { new StringContent("A"), "correct_answer" }
        }))).GetProperty("data").GetProperty("id").GetInt64();

    private async Task<long> LessonAsync(long subjectId)
        => (await Json(await _admin.PostAsync("/api/lesson", new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" }, { new StringContent(Unique("L")), "title" }
        }))).GetProperty("data").GetProperty("id").GetInt64();

    private async Task<long> QuizAsync(long lessonId, bool isRepeat)
        => (await Json(await _admin.PostAsync("/api/quiz", new MultipartFormDataContent
        {
            { new StringContent(lessonId.ToString()), "lesson_id" }, { new StringContent(Unique("Quiz")), "title" },
            { new StringContent(isRepeat ? "true" : "false"), "is_repeat" }
        }))).GetProperty("data").GetProperty("id").GetInt64();

    private async Task<(HttpClient student, long quizId, long questionId)> SetupAsync(bool isRepeat)
    {
        var subjectId = await SubjectAsync();
        var q = await QuestionAsync(subjectId);
        var lessonId = await LessonAsync(subjectId);
        var quizId = await QuizAsync(lessonId, isRepeat);
        await _admin.PostAsJsonAsync($"/api/quiz/{quizId}/questions", new { questionIds = new[] { q } });

        var groupId = (await Json(await _admin.PostAsync("/api/group", new MultipartFormDataContent { { new StringContent(Unique("G")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { subjectId } });

        var reg = await _admin.PostAsJsonAsync("/api/auth/register", new { fullName = "R", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var d = (await Json(reg)).GetProperty("data");
        var studentId = d.GetProperty("user").GetProperty("id").GetInt64();
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { studentId } });

        var student = factory.CreateClient();
        student.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", d.GetProperty("token").GetString());
        return (student, quizId, q);
    }

    private static async Task CompleteAsync(HttpClient student, long quizId, long questionId)
    {
        var start = await Json(await student.PostAsJsonAsync("/api/student-quiz/start", new { quizId }));
        var attemptId = start.GetProperty("data").GetProperty("attemptId").GetInt64();
        await student.PostAsJsonAsync("/api/student-quiz/complete", new
        {
            attemptId,
            answers = new object[] { new { questionId, answer = "A" } }
        });
    }

    [Fact]
    public async Task Non_repeat_quiz_cannot_be_started_after_completion()
    {
        var (student, quizId, q) = await SetupAsync(isRepeat: false);
        await CompleteAsync(student, quizId, q);

        var second = await student.PostAsJsonAsync("/api/student-quiz/start", new { quizId });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Repeat_quiz_allows_multiple_attempts_with_attempt_time()
    {
        var (student, quizId, q) = await SetupAsync(isRepeat: true);
        await CompleteAsync(student, quizId, q);

        var second = await student.PostAsJsonAsync("/api/student-quiz/start", new { quizId });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(2, (await Json(second)).GetProperty("data").GetProperty("attemptTime").GetInt32());
    }
}
