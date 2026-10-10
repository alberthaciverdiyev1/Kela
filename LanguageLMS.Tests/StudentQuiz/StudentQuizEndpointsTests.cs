using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.StudentQuiz;

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

    private async Task<long> CreateSubjectAsync()
    {
        var r = await _client.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("Subj")), "name" } });
        return (await Json(r)).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateQuestionAsync(long subjectId, string correct)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent(Unique("Q")), "title" },
            { new StringContent("a"), "variant_a" }, { new StringContent("b"), "variant_b" },
            { new StringContent("c"), "variant_c" }, { new StringContent("d"), "variant_d" },
            { new StringContent(correct), "correct_answer" }
        };
        return (await Json(await _client.PostAsync("/api/question", form))).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateLessonAsync(long subjectId)
    {
        var f = new MultipartFormDataContent { { new StringContent(subjectId.ToString()), "subject_id" }, { new StringContent(Unique("L")), "title" } };
        return (await Json(await _client.PostAsync("/api/lesson", f))).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateQuizAsync(long lessonId)
    {
        var f = new MultipartFormDataContent { { new StringContent(lessonId.ToString()), "lesson_id" }, { new StringContent(Unique("Quiz")), "title" } };
        return (await Json(await _client.PostAsync("/api/quiz", f))).GetProperty("data").GetProperty("id").GetInt64();
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
        var subjectId = await CreateSubjectAsync();
        var q1 = await CreateQuestionAsync(subjectId, "A");
        var q2 = await CreateQuestionAsync(subjectId, "B");
        var lessonId = await CreateLessonAsync(subjectId);
        var quizId = await CreateQuizAsync(lessonId);
        await _client.PostAsJsonAsync($"/api/quiz/{quizId}/questions", new { questionIds = new[] { q1, q2 } });

        var groupId = (await Json(await _client.PostAsync("/api/group", new MultipartFormDataContent { { new StringContent(Unique("G")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();
        await _client.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { subjectId } });
        await _client.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { studentId } });

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
        var subjectId = await CreateSubjectAsync();
        await CreateQuestionAsync(subjectId, "A");
        var lessonId = await CreateLessonAsync(subjectId);
        var quizId = await CreateQuizAsync(lessonId);

        var student = Authed(token);
        var response = await student.PostAsJsonAsync("/api/student-quiz/start", new { quizId });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Requires_auth()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/student-quiz/results")).StatusCode);
}
