using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Auth;

[Collection("api")]
public class AuthEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static string Email() => $"user-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Register_login_and_access_protected_endpoint()
    {
        var email = Email();

        var register = await _client.PostAsJsonAsync("/api/auth/register",
            new { fullName = "Test User", email, password = "secret123" });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(
            (await ReadJsonAsync(register)).GetProperty("data").GetProperty("token").GetString()));

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "secret123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await ReadJsonAsync(login)).GetProperty("data").GetProperty("token").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Equal(email, (await ReadJsonAsync(me)).GetProperty("data").GetProperty("email").GetString());
    }

    [Fact]
    public async Task Me_without_token_returns_unauthorized()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/auth/me")).StatusCode);

    [Fact]
    public async Task Login_with_wrong_password_returns_unauthorized()
    {
        var email = Email();
        await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "U", email, password = "secret123" });

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "wrongpass" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_bad_request()
    {
        var email = Email();
        await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "U", email, password = "secret123" });

        var second = await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "U", email, password = "secret123" });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Register_with_short_password_returns_validation()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { fullName = "U", email = Email(), password = "123" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
