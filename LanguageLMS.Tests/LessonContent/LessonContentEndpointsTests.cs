using System.Net.Http.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LanguageLMS.Tests.LessonContent;

[Collection("api")]
public class LessonContentEndpointsTests(ApiFactory factory)
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

    private async Task<long> CreateLessonAsync(long subjectId)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent(Unique("Lesson")), "title" }
        };
        var r = await _client.PostAsync("/api/lesson", form);
        return (await ReadJsonAsync(r)).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateContentAsync(long lessonId, string type = "video")
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(lessonId.ToString()), "lesson_id" },
            { new StringContent(Unique("Content")), "title" },
            { new StringContent(type), "type" }
        };
        var file = new ByteArrayContent(new byte[10]);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(file, "file", "sample.mp4");

        var response = await _client.PostAsync("/api/lesson-content", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("data").GetProperty("id").GetInt64();
    }

    [Fact]
    public async Task Create_list_update_delete_flow()
    {
        var lessonId = await CreateLessonAsync(await CreateSubjectAsync());
        var id = await CreateContentAsync(lessonId, "video");

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/lesson-content?lesson_id={lessonId}"));
        var item = Items(list).First(x => x.GetProperty("id").GetInt64() == id);
        Assert.Equal("video", item.GetProperty("type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("url").GetString()));

        var updateForm = new MultipartFormDataContent
        {
            { new StringContent(Unique("Updated")), "title" },
            { new StringContent("false"), "is_active" }
        };
        var update = await _client.PutAsync($"/api/lesson-content/{id}", updateForm);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.False((await ReadJsonAsync(update)).GetProperty("data").GetProperty("isActive").GetBoolean());

        Assert.Equal(HttpStatusCode.OK, (await _client.DeleteAsync($"/api/lesson-content/{id}")).StatusCode);
    }

    [Theory]
    [InlineData("video")]
    [InlineData("file")]
    [InlineData("voice")]
    public async Task Create_supports_all_types(string type)
    {
        var lessonId = await CreateLessonAsync(await CreateSubjectAsync());
        var id = await CreateContentAsync(lessonId, type);

        var show = await ReadJsonAsync(await _client.GetAsync($"/api/lesson-content/{id}"));
        Assert.Equal(type, show.GetProperty("data").GetProperty("type").GetString());
    }

    [Fact]
    public async Task Create_without_file_returns_validation_error()
    {
        var lessonId = await CreateLessonAsync(await CreateSubjectAsync());
        var form = new MultipartFormDataContent
        {
            { new StringContent(lessonId.ToString()), "lesson_id" },
            { new StringContent("No file"), "title" },
            { new StringContent("video"), "type" }
        };
        var response = await _client.PostAsync("/api/lesson-content", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_id_returns_not_found()
        => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/lesson-content/999999")).StatusCode);

    [Fact]
    public async Task Create_with_path_instead_of_file()
    {
        var lessonId = await CreateLessonAsync(await CreateSubjectAsync());
        var form = new MultipartFormDataContent
        {
            { new StringContent(lessonId.ToString()), "lesson_id" },
            { new StringContent(Unique("Video")), "title" },
            { new StringContent("video"), "type" },
            { new StringContent("video/direct-upload.mp4"), "path" }
        };
        var response = await _client.PostAsync("/api/lesson-content", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var data = (await ReadJsonAsync(response)).GetProperty("data");
        Assert.Equal("video", data.GetProperty("type").GetString());
        Assert.Contains("direct-upload.mp4", data.GetProperty("url").GetString());
    }

    [Fact]
    public async Task Upload_url_requires_presigned_support()
    {
        var lessonId = await CreateLessonAsync(await CreateSubjectAsync());
        // test mühitində Local storage → presigned dəstəklənmir → 400
        var response = await _client.PostAsJsonAsync("/api/lesson-content/upload-url",
            new { lessonId, type = "video", fileName = "a.mp4", contentType = "video/mp4" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
