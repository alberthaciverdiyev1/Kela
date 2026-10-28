using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Edutor.Tests.StudentApp;

[Collection("api")]
public class MembershipWindowTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement j) => j.GetProperty("data").EnumerateArray();

    private async Task<(long id, string token)> RegisterStudentAsync()
    {
        var reg = await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "W", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var d = (await Json(reg)).GetProperty("data");
        return (d.GetProperty("user").GetProperty("id").GetInt64(), d.GetProperty("token").GetString()!);
    }

    private HttpClient Authed(string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    [Fact]
    public async Task Future_membership_window_hides_current_lessons_span_shows_them()
    {
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subj"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("L"));
        var groupId = await TestData.GroupAsync(factory, Unique("G"));
        await TestData.AssignSubjectsAsync(factory, groupId, subjectId);

        var (studentId, token) = await RegisterStudentAsync();

        // gələcək pəncərə: dərs (indi yaradılıb) görünməməlidir
        var future = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var futureEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(40));
        await TestData.AssignStudentsAsync(factory, groupId, new[] { studentId }, future, futureEnd);

        var student = Authed(token);

        var hidden = await Json(await student.GetAsync($"/api/student/subjects/{subjectId}/lessons"));
        Assert.DoesNotContain(Items(hidden), x => x.GetProperty("id").GetInt64() == lessonId);

        // aralığı əhatə edən pəncərə: görünməlidir
        var past = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        await TestData.AssignStudentsAsync(factory, groupId, new[] { studentId }, past, futureEnd);

        var visible = await Json(await student.GetAsync($"/api/student/subjects/{subjectId}/lessons"));
        Assert.Contains(Items(visible), x => x.GetProperty("id").GetInt64() == lessonId);
    }

    [Fact]
    public async Task Content_count_returns_flags()
    {
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subj"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("L"));
        var groupId = await TestData.GroupAsync(factory, Unique("G"));
        await TestData.AssignSubjectsAsync(factory, groupId, subjectId);

        var (studentId, token) = await RegisterStudentAsync();
        await TestData.AssignStudentsAsync(factory, groupId, new[] { studentId });

        var student = Authed(token);

        var counts = await Json(await student.GetAsync($"/api/student/lessons/{lessonId}/content-count"));
        Assert.Equal(1, counts.GetProperty("data").GetProperty("topicExplain").GetInt32());
        Assert.Equal(1, counts.GetProperty("data").GetProperty("generalQuiz").GetInt32());
    }
}
