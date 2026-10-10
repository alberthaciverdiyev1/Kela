using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Admin;

[Collection("api")]
public class AdminFeaturesEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement j)
        => j.GetProperty("data").GetProperty("data").EnumerateArray();

    private async Task<long> CreateSubjectAsync()
        => (await Json(await _admin.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("S")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();

    [Fact]
    public async Task Dashboard_returns_stats()
    {
        var resp = await _admin.GetAsync("/api/dashboard");
        var body = await resp.Content.ReadAsStringAsync();
        Assert.True(resp.IsSuccessStatusCode, body);
        var data = JsonDocument.Parse(body).RootElement.GetProperty("data");
        Assert.True(data.GetProperty("students").GetProperty("total").GetInt32() >= 0);
        Assert.True(data.GetProperty("content").GetProperty("subjects").GetInt32() >= 0);
    }

    [Fact]
    public async Task Students_list_status_and_export()
    {
        var email = $"{Unique("s")}@ex.com";
        await _admin.PostAsJsonAsync("/api/auth/register", new { fullName = "Listed Student", email, password = "secret123" });

        var list = await Json(await _admin.GetAsync($"/api/students?search=Listed"));
        var student = Items(list).First(x => x.GetProperty("email").GetString() == email);
        var id = student.GetProperty("id").GetInt64();

        var status = await _admin.PostAsJsonAsync($"/api/students/{id}/status", new { isActive = false });
        Assert.False((await Json(status)).GetProperty("data").GetProperty("isActive").GetBoolean());

        var export = await _admin.GetAsync("/api/students/export");
        Assert.Equal(HttpStatusCode.OK, export.StatusCode);
        Assert.Contains("spreadsheetml", export.Content.Headers.ContentType!.ToString());
    }

    [Fact]
    public async Task Bulk_delete_subjects()
    {
        var s1 = await CreateSubjectAsync();
        var s2 = await CreateSubjectAsync();

        var bulk = await _admin.PostAsJsonAsync("/api/subject/bulk", new { ids = new[] { s1, s2 }, action = "delete" });
        Assert.Equal(HttpStatusCode.OK, bulk.StatusCode);

        var deleted = await Json(await _admin.GetAsync("/api/subject?is_deleted=1&per_page=200"));
        var ids = Items(deleted).Select(x => x.GetProperty("id").GetInt64()).ToHashSet();
        Assert.Contains(s1, ids);
        Assert.Contains(s2, ids);

        // restore
        await _admin.PostAsJsonAsync("/api/subject/bulk", new { ids = new[] { s1 }, action = "restore" });
        var active = await Json(await _admin.GetAsync("/api/subject?per_page=200&search=" + Unique("nope")));
        _ = active;
    }

    [Fact]
    public async Task Bulk_requires_teacher()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().PostAsJsonAsync("/api/subject/bulk", new { ids = new[] { 1L }, action = "delete" })).StatusCode);

    [Fact]
    public async Task Dashboard_requires_teacher()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/dashboard")).StatusCode);
}
