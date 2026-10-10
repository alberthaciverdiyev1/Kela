using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Quiz;

[Collection("api")]
public class QuizEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement json)
        => json.GetProperty("data").GetProperty("data").EnumerateArray();

    private async Task<long> CreateSubjectAsync()
    {
        var r = await _client.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("Subject")), "name" } });
        return (await ReadJsonAsync(r)).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateQuestionAsync(long subjectId)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent(Unique("Q")), "title" },
            { new StringContent("a"), "variant_a" }, { new StringContent("b"), "variant_b" },
            { new StringContent("c"), "variant_c" }, { new StringContent("d"), "variant_d" },
            { new StringContent("A"), "correct_answer" }
        };
        return (await ReadJsonAsync(await _client.PostAsync("/api/question", form))).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateLessonAsync(long subjectId)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent(Unique("Lesson")), "title" }
        };
        return (await ReadJsonAsync(await _client.PostAsync("/api/lesson", form))).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateQuizAsync(long lessonId, string? title = null)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(lessonId.ToString()), "lesson_id" },
            { new StringContent(title ?? Unique("Quiz")), "title" },
            { new StringContent("30"), "duration_minutes" },
            { new StringContent("true"), "is_active" }
        };
        var r = await _client.PostAsync("/api/quiz", form);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await ReadJsonAsync(r)).GetProperty("data").GetProperty("id").GetInt64();
    }

    [Fact]
    public async Task Create_assign_list_remove_questions()
    {
        var subjectId = await CreateSubjectAsync();
        var lessonId = await CreateLessonAsync(subjectId);
        var q1 = await CreateQuestionAsync(subjectId);
        var q2 = await CreateQuestionAsync(subjectId);

        var quizId = await CreateQuizAsync(lessonId);

        var assign = await _client.PostAsJsonAsync($"/api/quiz/{quizId}/questions", new { questionIds = new[] { q1, q2 } });
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);
        Assert.Equal(2, (await ReadJsonAsync(assign)).GetProperty("data").GetArrayLength());

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/quiz/{quizId}/questions"));
        Assert.Equal(2, list.GetProperty("data").GetArrayLength());

        // question count quiz listesinde
        var quizzes = await ReadJsonAsync(await _client.GetAsync($"/api/quiz?lesson_id={lessonId}"));
        var item = Items(quizzes).First(x => x.GetProperty("id").GetInt64() == quizId);
        Assert.Equal(2, item.GetProperty("questionCount").GetInt32());

        var remove = await _client.DeleteAsync($"/api/quiz/{quizId}/questions/{q1}");
        Assert.Equal(HttpStatusCode.OK, remove.StatusCode);

        var after = await ReadJsonAsync(await _client.GetAsync($"/api/quiz/{quizId}/questions"));
        Assert.Equal(1, after.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task Assign_unknown_question_returns_bad_request()
    {
        var subjectId = await CreateSubjectAsync();
        var lessonId = await CreateLessonAsync(subjectId);
        var quizId = await CreateQuizAsync(lessonId);

        var response = await _client.PostAsJsonAsync($"/api/quiz/{quizId}/questions", new { questionIds = new[] { 999999L } });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_id_returns_not_found()
        => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/quiz/999999")).StatusCode);
}
