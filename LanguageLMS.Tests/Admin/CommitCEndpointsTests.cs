using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Admin;

[Collection("api")]
public class CommitCEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    [Fact]
    public async Task Generate_students()
    {
        var response = await _admin.PostAsJsonAsync("/api/students/generate", new { count = 3, fullNamePrefix = "Auto" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, (await Json(response)).GetProperty("data").GetProperty("created").GetInt32());
    }

    [Fact]
    public async Task Students_export_is_xlsx()
    {
        var export = await _admin.GetAsync("/api/students/export");
        Assert.Equal(HttpStatusCode.OK, export.StatusCode);
        Assert.Contains("spreadsheetml", export.Content.Headers.ContentType!.ToString());
    }

    [Fact]
    public async Task Admin_can_list_and_unbind_student_devices()
    {
        var email = $"{Unique("d")}@ex.com";
        var reg = await _admin.PostAsJsonAsync("/api/auth/register", new { fullName = "D", email, password = "secret123" });
        var studentId = (await Json(reg)).GetProperty("data").GetProperty("user").GetProperty("id").GetInt64();

        // login with device -> binds
        await _admin.PostAsJsonAsync("/api/auth/login", new { email, password = "secret123", deviceId = "dev-abc" });

        var devices = await Json(await _admin.GetAsync($"/api/students/{studentId}/devices"));
        Assert.Contains(devices.GetProperty("data").EnumerateArray(), x => x.GetProperty("deviceId").GetString() == "dev-abc");

        Assert.Equal(HttpStatusCode.OK, (await _admin.DeleteAsync($"/api/students/{studentId}/devices")).StatusCode);

        var after = await Json(await _admin.GetAsync($"/api/students/{studentId}/devices"));
        Assert.Empty(after.GetProperty("data").EnumerateArray());
    }
}
