using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.User;

[Collection("api")]
public class UserAuthEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Email() => $"teacher-{Guid.NewGuid():N}@example.com";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<string> CreateAndLoginAsync(string email, string password = "secret123")
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("Ali"), "name" },
            { new StringContent(email), "email" },
            { new StringContent("teacher"), "type" },
            { new StringContent(password), "password" }
        };
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsync("/api/user", form)).StatusCode);

        var login = await _client.PostAsJsonAsync("/api/user-auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        return (await ReadJsonAsync(login)).GetProperty("data").GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task Login_and_access_me()
    {
        var email = Email();
        var token = await CreateAndLoginAsync(email);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/user-auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Equal(email, (await ReadJsonAsync(me)).GetProperty("data").GetProperty("email").GetString());
    }

    [Fact]
    public async Task Me_without_token_returns_unauthorized()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/user-auth/me")).StatusCode);

    [Fact]
    public async Task Login_with_wrong_password_returns_unauthorized()
    {
        var email = Email();
        await CreateAndLoginAsync(email);

        var login = await _client.PostAsJsonAsync("/api/user-auth/login", new { email, password = "wrongpass" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Student_token_cannot_access_user_endpoints()
    {
        // student login
        var email = $"student-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "S", email, password = "secret123" });
        var studentToken = (await ReadJsonAsync(register)).GetProperty("data").GetProperty("token").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/user-auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
        var me = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, me.StatusCode);
    }
}
