using System.Net;
using System.Text.Json;

namespace LanguageLMS.Tests.Lesson;

[Collection("api")]
public class LessonEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement json)
        => json.GetProperty("data").GetProperty("data").EnumerateArray();

    private async Task<long> CreateSubjectAsync()
    {
        var response = await _client.PostAsync("/api/subject", new MultipartFormDataContent
        {
            { new StringContent(Unique("Subject")), "name" }
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateLessonAsync(long subjectId, string title, int sortOrder = 0, bool isActive = true)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent(title), "title" },
            { new StringContent(sortOrder.ToString()), "sort_order" },
            { new StringContent(isActive ? "true" : "false"), "is_active" }
        };
        var response = await _client.PostAsync("/api/lesson", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("data").GetProperty("id").GetInt64();
    }

    [Fact]
    public async Task Full_crud_flow()
    {
        var subjectId = await CreateSubjectAsync();
        var id = await CreateLessonAsync(subjectId, Unique("Lesson"));

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/lesson?subject_id={subjectId}"));
        Assert.Contains(Items(list), x => x.GetProperty("id").GetInt64() == id);

        var updateForm = new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent(Unique("Lesson-upd")), "title" },
            { new StringContent("false"), "is_active" }
        };
        var update = await _client.PutAsync($"/api/lesson/{id}", updateForm);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.False((await ReadJsonAsync(update)).GetProperty("data").GetProperty("isActive").GetBoolean());

        Assert.Equal(HttpStatusCode.OK, (await _client.DeleteAsync($"/api/lesson/{id}")).StatusCode);
    }

    [Fact]
    public async Task IsActive_filter_works()
    {
        var subjectId = await CreateSubjectAsync();
        await CreateLessonAsync(subjectId, Unique("Active"), isActive: true);
        await CreateLessonAsync(subjectId, Unique("Inactive"), isActive: false);

        var active = await ReadJsonAsync(await _client.GetAsync($"/api/lesson?subject_id={subjectId}&is_active=true"));
        Assert.All(Items(active), x => Assert.True(x.GetProperty("isActive").GetBoolean()));
    }

    [Fact]
    public async Task Create_with_unknown_subject_returns_bad_request()
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("999999"), "subject_id" },
            { new StringContent("Lesson"), "title" }
        };
        var response = await _client.PostAsync("/api/lesson", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_id_returns_not_found()
        => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/lesson/999999")).StatusCode);
}
