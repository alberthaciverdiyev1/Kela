using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LanguageLMS.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LanguageLMS.Tests.ScreenGuard;

[Collection("api")]
public class ScreenGuardEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<string> RegisterStudentAsync()
    {
        var r = await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "S", email = $"{Unique("s")}@ex.com", password = "secret123" });
        return (await Json(r)).GetProperty("data").GetProperty("token").GetString()!;
    }

    private HttpClient Authed(string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    [Fact]
    public async Task Violation_is_logged_for_student()
    {
        var student = Authed(await RegisterStudentAsync());
        var path = Unique("/exams");

        var r = await student.PostAsJsonAsync("/api/screen-guard/violations", new { type = "window-blur", path });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.ScreenViolations.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Type == "window-blur" && x.Path == path);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Empty_violation_type_is_ignored()
    {
        var student = Authed(await RegisterStudentAsync());
        var r = await student.PostAsJsonAsync("/api/screen-guard/violations", new { type = "  ", path = "/x" });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Requires_auth()
        => Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.CreateClient().PostAsJsonAsync("/api/screen-guard/violations", new { type = "x", path = "/" })).StatusCode);
}
