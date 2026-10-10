using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.StudentApp;

[Collection("api")]
public class MembershipWindowTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement j) => j.GetProperty("data").EnumerateArray();

    [Fact]
    public async Task Future_membership_window_hides_current_lessons_span_shows_them()
    {
        var subjectId = (await Json(await _admin.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("Subj")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();
        var lessonId = (await Json(await _admin.PostAsync("/api/lesson", new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" }, { new StringContent(Unique("L")), "title" }
        }))).GetProperty("data").GetProperty("id").GetInt64();

        var groupId = (await Json(await _admin.PostAsync("/api/group", new MultipartFormDataContent { { new StringContent(Unique("G")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { subjectId } });

        var reg = await _admin.PostAsJsonAsync("/api/auth/register", new { fullName = "W", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var d = (await Json(reg)).GetProperty("data");
        var studentId = d.GetProperty("user").GetProperty("id").GetInt64();
        var token = d.GetProperty("token").GetString()!;

        // gələcək pəncərə: dərs (indi yaradılıb) görünməməlidir
        var future = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd");
        var futureEnd = DateTime.UtcNow.AddDays(40).ToString("yyyy-MM-dd");
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { studentId }, startDate = future, endDate = futureEnd });

        var student = factory.CreateClient();
        student.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var hidden = await Json(await student.GetAsync($"/api/student/subjects/{subjectId}/lessons"));
        Assert.DoesNotContain(Items(hidden), x => x.GetProperty("id").GetInt64() == lessonId);

        // aralığı əhatə edən pəncərə: görünməlidir
        var past = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd");
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { studentId }, startDate = past, endDate = futureEnd });

        var visible = await Json(await student.GetAsync($"/api/student/subjects/{subjectId}/lessons"));
        Assert.Contains(Items(visible), x => x.GetProperty("id").GetInt64() == lessonId);
    }

    [Fact]
    public async Task Content_count_returns_flags()
    {
        var subjectId = (await Json(await _admin.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("Subj")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();
        var lessonId = (await Json(await _admin.PostAsync("/api/lesson", new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" }, { new StringContent(Unique("L")), "title" }
        }))).GetProperty("data").GetProperty("id").GetInt64();

        var groupId = (await Json(await _admin.PostAsync("/api/group", new MultipartFormDataContent { { new StringContent(Unique("G")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { subjectId } });

        var reg = await _admin.PostAsJsonAsync("/api/auth/register", new { fullName = "W", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var d = (await Json(reg)).GetProperty("data");
        var studentId = d.GetProperty("user").GetProperty("id").GetInt64();
        var token = d.GetProperty("token").GetString()!;
        await _admin.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { studentId } });

        var student = factory.CreateClient();
        student.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var counts = await Json(await student.GetAsync($"/api/student/lessons/{lessonId}/content-count"));
        Assert.Equal(1, counts.GetProperty("data").GetProperty("topicExplain").GetInt32());
        Assert.Equal(1, counts.GetProperty("data").GetProperty("generalQuiz").GetInt32());
    }
}
