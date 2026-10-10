using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Auth;

[Collection("api")]
public class DeviceLockEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    [Fact]
    public async Task First_login_binds_device_other_device_is_rejected()
    {
        var email = $"d-{Guid.NewGuid():N}@ex.com";
        await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "D", email, password = "secret123" });

        var first = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "secret123", deviceId = "device-1" });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var other = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "secret123", deviceId = "device-2" });
        Assert.Equal(HttpStatusCode.Forbidden, other.StatusCode);

        var same = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "secret123", deviceId = "device-1" });
        Assert.Equal(HttpStatusCode.OK, same.StatusCode);
    }

    [Fact]
    public async Task Login_without_device_id_is_allowed()
    {
        var email = $"d-{Guid.NewGuid():N}@ex.com";
        await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "D", email, password = "secret123" });
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "secret123" })).StatusCode);
    }
}
