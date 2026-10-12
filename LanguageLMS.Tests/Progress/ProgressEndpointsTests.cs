using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Progress;

[Collection("api")]
public class ProgressEndpointsTests(ApiFactory factory)
{
    // şagird qeydiyyatı üçün client (aranjiman TestData servisləri ilə edilir)
    private readonly HttpClient _client = factory.TeacherClient();

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
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subject"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("Lesson"));
        var contentId = await TestData.LessonContentAsync(factory, lessonId, Unique("Content"));

        var groupId = await TestData.GroupAsync(factory, Unique("Group"));
        await TestData.AssignSubjectsAsync(factory, groupId, subjectId);
        await TestData.AssignStudentsAsync(factory, groupId, new[] { studentId });

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
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subject"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("Lesson"));
        var contentId = await TestData.LessonContentAsync(factory, lessonId, Unique("Content"));

        var student = Authed(token);
        var response = await student.PostAsJsonAsync("/api/progress", new { contentId });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Progress_requires_auth()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/progress")).StatusCode);
}
