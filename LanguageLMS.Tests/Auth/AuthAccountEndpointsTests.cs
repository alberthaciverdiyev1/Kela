using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LanguageLMS.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LanguageLMS.Tests.Auth;

[Collection("api")]
public class AuthAccountEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Email() => $"user-{Guid.NewGuid():N}@example.com";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<(string email, HttpClient client)> RegisterAsync()
    {
        var email = Email();
        var reg = await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "Orig", email, password = "secret123" });
        var token = (await Json(reg)).GetProperty("data").GetProperty("token").GetString()!;
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (email, client);
    }

    private async Task<string> LatestOtpAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var otp = await db.OtpCodes.Where(x => x.Email == email).OrderByDescending(x => x.Id).FirstAsync();
        return otp.Code;
    }

    [Fact]
    public async Task Change_name_phone_and_notification_status()
    {
        var (_, client) = await RegisterAsync();

        var name = await client.PutAsJsonAsync("/api/auth/change-full-name", new { fullName = "New Name" });
        Assert.Equal("New Name", (await Json(name)).GetProperty("data").GetProperty("fullName").GetString());

        var phone = await client.PutAsJsonAsync("/api/auth/change-phone", new { phone = "+994501112233" });
        Assert.Equal("+994501112233", (await Json(phone)).GetProperty("data").GetProperty("phone").GetString());

        var status = await client.PutAsJsonAsync("/api/auth/notification-status", new { enabled = true });
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
    }

    [Fact]
    public async Task Change_password_flow()
    {
        var (email, client) = await RegisterAsync();

        var wrong = await client.PostAsJsonAsync("/api/auth/change-password", new { currentPassword = "wrong", newPassword = "newsecret1" });
        Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);

        var ok = await client.PostAsJsonAsync("/api/auth/change-password", new { currentPassword = "secret123", newPassword = "newsecret1" });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "newsecret1" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Otp_flow_change_email_and_reset_password()
    {
        var (email, client) = await RegisterAsync();

        var newEmail = Email();
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/send-otp", new { email = newEmail })).StatusCode);
        var otp = await LatestOtpAsync(newEmail);

        var check = await client.PostAsJsonAsync("/api/auth/check-otp", new { email = newEmail, otp });
        Assert.True((await Json(check)).GetProperty("data").GetBoolean());

        var changeEmail = await client.PutAsJsonAsync("/api/auth/change-email", new { email = newEmail, otp });
        Assert.Equal(newEmail, (await Json(changeEmail)).GetProperty("data").GetProperty("email").GetString());

        // reset password via otp
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/send-otp", new { email = newEmail })).StatusCode);
        var otp2 = await LatestOtpAsync(newEmail);
        var reset = await client.PostAsJsonAsync("/api/auth/reset-password", new { email = newEmail, otp = otp2, newPassword = "resetpass1" });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email = newEmail, password = "resetpass1" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Check_otp_with_wrong_code_returns_false()
    {
        var (_, client) = await RegisterAsync();
        var email = Email();
        await client.PostAsJsonAsync("/api/auth/send-otp", new { email });

        var check = await client.PostAsJsonAsync("/api/auth/check-otp", new { email, otp = "0000" });
        Assert.Equal(HttpStatusCode.OK, check.StatusCode);
        Assert.False((await Json(check)).GetProperty("data").GetBoolean());
    }

    [Fact]
    public async Task Delete_account()
    {
        var (_, client) = await RegisterAsync();
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync("/api/auth/delete-account")).StatusCode);
    }
}
