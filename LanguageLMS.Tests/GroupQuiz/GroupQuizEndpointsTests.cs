using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.GroupQuiz;

[Collection("api")]
public class GroupQuizEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<long> CreateGeneralQuizAsync()
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(Unique("GeneralQuiz")), "title" },
            { new StringContent("general"), "type" },
            { new StringContent("true"), "is_active" }
        };
        var r = await _client.PostAsync("/api/quiz", form);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await Json(r)).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateGroupAsync()
    {
        var r = await _client.PostAsync("/api/group", new MultipartFormDataContent { { new StringContent(Unique("G")), "name" } });
        return (await Json(r)).GetProperty("data").GetProperty("id").GetInt64();
    }

    [Fact]
    public async Task Assign_quiz_to_group_and_list()
    {
        var quizId = await CreateGeneralQuizAsync();
        var groupId = await CreateGroupAsync();

        var assign = await _client.PostAsJsonAsync($"/api/group/{groupId}/quizzes", new { quizIds = new[] { quizId } });
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);
        Assert.Contains((await Json(assign)).GetProperty("data").EnumerateArray(),
            x => x.GetProperty("id").GetInt64() == quizId);

        var list = await Json(await _client.GetAsync($"/api/group/{groupId}/quizzes"));
        Assert.Contains(list.GetProperty("data").EnumerateArray(), x => x.GetProperty("id").GetInt64() == quizId);

        var remove = await _client.DeleteAsync($"/api/group/{groupId}/quizzes/{quizId}");
        Assert.Equal(HttpStatusCode.OK, remove.StatusCode);

        var after = await Json(await _client.GetAsync($"/api/group/{groupId}/quizzes"));
        Assert.DoesNotContain(after.GetProperty("data").EnumerateArray(), x => x.GetProperty("id").GetInt64() == quizId);
    }

    [Fact]
    public async Task Student_sees_group_quizzes()
    {
        var quizId = await CreateGeneralQuizAsync();
        var groupId = await CreateGroupAsync();

        var reg = await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "S", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var data = (await Json(reg)).GetProperty("data");
        var studentId = data.GetProperty("user").GetProperty("id").GetInt64();
        var token = data.GetProperty("token").GetString()!;

        await _client.PostAsJsonAsync($"/api/group/{groupId}/quizzes", new { quizIds = new[] { quizId } });
        await _client.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { studentId } });

        var student = factory.CreateClient();
        student.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var quizzes = await Json(await student.GetAsync("/api/student/quizzes"));
        Assert.Contains(quizzes.GetProperty("data").EnumerateArray(), x => x.GetProperty("id").GetInt64() == quizId);
    }

    [Fact]
    public async Task General_quiz_without_lesson_is_created()
    {
        var quizId = await CreateGeneralQuizAsync();
        var show = await Json(await _client.GetAsync($"/api/quiz/{quizId}"));
        Assert.Equal("general", show.GetProperty("data").GetProperty("type").GetString());
        Assert.Equal(JsonValueKind.Null, show.GetProperty("data").GetProperty("lessonId").ValueKind);
    }
}
