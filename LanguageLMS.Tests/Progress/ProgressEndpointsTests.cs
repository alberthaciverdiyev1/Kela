using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Progress;

[Collection("api")]
public class ProgressEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<(long id, string token)> RegisterStudentAsync()
    {
        var r = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Student",
            email = $"student-{Guid.NewGuid():N}@example.com",
            password = "secret123"
        });
        var data = (await ReadJsonAsync(r)).GetProperty("data");
        return (data.GetProperty("user").GetProperty("id").GetInt64(), data.GetProperty("token").GetString()!);
    }

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
        return (await ReadJsonAsync(await _client.PostAsync("/api/lesson", form))).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateContentAsync(long lessonId)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(lessonId.ToString()), "lesson_id" },
            { new StringContent(Unique("Content")), "title" },
            { new StringContent("video"), "type" }
        };
        var file = new ByteArrayContent(new byte[10]);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(file, "file", "v.mp4");
        return (await ReadJsonAsync(await _client.PostAsync("/api/lesson-content", form))).GetProperty("data").GetProperty("id").GetInt64();
    }

    private HttpClient Authed(string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    [Fact]
    public async Task Mark_content_and_read_progress_and_statistics()
    {
        var (studentId, token) = await RegisterStudentAsync();
        var subjectId = await CreateSubjectAsync();
        var lessonId = await CreateLessonAsync(subjectId);
        var contentId = await CreateContentAsync(lessonId);

        // group + assign
        var groupId = (await ReadJsonAsync(await _client.PostAsync("/api/group",
            new MultipartFormDataContent { { new StringContent(Unique("Group")), "name" } })))
            .GetProperty("data").GetProperty("id").GetInt64();
        await _client.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { subjectId } });
        await _client.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { studentId } });

        var student = Authed(token);

        var before = await ReadJsonAsync(await student.GetAsync("/api/progress"));
        var beforeItem = before.GetProperty("data").GetProperty("items").EnumerateArray()
            .First(x => x.GetProperty("lessonId").GetInt64() == lessonId);
        Assert.Equal(0, beforeItem.GetProperty("completedContents").GetInt32());

        var mark = await student.PostAsJsonAsync("/api/progress", new { contentId });
        Assert.Equal(HttpStatusCode.OK, mark.StatusCode);

        var after = await ReadJsonAsync(await student.GetAsync("/api/progress"));
        var afterItem = after.GetProperty("data").GetProperty("items").EnumerateArray()
            .First(x => x.GetProperty("lessonId").GetInt64() == lessonId);
        Assert.Equal(1, afterItem.GetProperty("completedContents").GetInt32());
        Assert.Equal(100, afterItem.GetProperty("progressPercent").GetInt32());

        var stats = await ReadJsonAsync(await student.GetAsync("/api/statistics"));
        var overview = stats.GetProperty("data").GetProperty("overview");
        Assert.Equal(1, overview.GetProperty("completedContents").GetInt32());
        Assert.Equal(100, overview.GetProperty("completionPercent").GetInt32());
    }

    [Fact]
    public async Task Marking_inaccessible_content_returns_forbidden()
    {
        var (_, token) = await RegisterStudentAsync();
        var subjectId = await CreateSubjectAsync();
        var lessonId = await CreateLessonAsync(subjectId);
        var contentId = await CreateContentAsync(lessonId);

        var student = Authed(token);
        var response = await student.PostAsJsonAsync("/api/progress", new { contentId });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Progress_requires_auth()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/progress")).StatusCode);
}
