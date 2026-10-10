using System.Net;
using System.Text.Json;

namespace LanguageLMS.Tests.Setting;

[Collection("api")]
public class SettingEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    [Fact]
    public async Task Get_and_update_setting()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/setting")).StatusCode);

        var number = $"+994{Guid.NewGuid():N}"[..12];
        var form = new MultipartFormDataContent { { new StringContent(number), "whatsappNumber" } };

        var update = await _client.PutAsync("/api/setting", form);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var after = await ReadJsonAsync(await _client.GetAsync("/api/setting"));
        Assert.Equal(number, after.GetProperty("data").GetProperty("whatsappNumber").GetString());
    }

    [Fact]
    public async Task App_version_is_public()
    {
        var response = await _client.GetAsync("/api/app-version");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await ReadJsonAsync(response)).GetProperty("data").TryGetProperty("android", out _));
    }
}
