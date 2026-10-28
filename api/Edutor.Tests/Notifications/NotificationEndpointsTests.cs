using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Edutor.Core.Data;
using Edutor.Core.Modules.Notification.Abstractions;
using Edutor.Core.Modules.Notification.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Tests.Notifications;

[Collection("api")]
public class NotificationEndpointsTests(ApiFactory factory)
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
    public async Task Student_sees_broadcast_notifications()
    {
        var title = Unique("Bildiriş");
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            await svc.CreateAsync(new CreateNotificationRequest { Title = title, Body = "Salam", All = true }, null);
        }

        var student = Authed(await RegisterStudentAsync());
        var r = await student.GetAsync("/api/student/notifications?page=1&perPage=20");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        var data = (await Json(r)).GetProperty("data");
        Assert.True(data.TryGetProperty("currentPage", out _));
        Assert.True(data.TryGetProperty("lastPage", out _));
        Assert.Contains(data.GetProperty("data").EnumerateArray(),
            x => x.GetProperty("title").GetString() == title);
    }

    [Fact]
    public async Task Device_token_is_saved_and_removed()
    {
        var student = Authed(await RegisterStudentAsync());
        var token = Unique("tok");

        Assert.Equal(HttpStatusCode.OK,
            (await student.PostAsJsonAsync("/api/student/notifications/token", new { token, deviceType = "web" })).StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await db.NotificationTokens.IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Token == token);
            Assert.NotNull(saved);
            Assert.True(saved!.IsActive);
        }

        Assert.Equal(HttpStatusCode.OK,
            (await student.DeleteAsync($"/api/student/notifications/token?token={Uri.EscapeDataString(token)}")).StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await db.NotificationTokens.IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Token == token);
            Assert.False(saved!.IsActive);
        }
    }

    [Fact]
    public async Task Requires_auth()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/student/notifications")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.CreateClient().PostAsJsonAsync("/api/student/notifications/token", new { token = "x" })).StatusCode);
    }
}
