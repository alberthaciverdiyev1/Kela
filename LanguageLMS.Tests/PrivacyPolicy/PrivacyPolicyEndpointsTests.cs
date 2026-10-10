using System.Net;
using System.Text.Json;

namespace LanguageLMS.Tests.PrivacyPolicy;

[Collection("api")]
public class PrivacyPolicyEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _teacher = factory.TeacherClient();

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    [Fact]
    public async Task Upsert_then_public_get()
    {
        var html = "<h1>Privacy</h1><p>content</p>";
        var form = new MultipartFormDataContent
        {
            { new StringContent("privacy"), "type" },
            { new StringContent(html), "content" }
        };
        var upsert = await _teacher.PutAsync("/api/privacy-policy", form);
        Assert.Equal(HttpStatusCode.OK, upsert.StatusCode);

        // GET public (auth-sız)
        var publicClient = factory.CreateClient();
        var get = await Json(await publicClient.GetAsync("/api/privacy-policy/privacy"));
        Assert.Equal(html, get.GetProperty("data").GetProperty("content").GetString());

        var list = await Json(await publicClient.GetAsync("/api/privacy-policy"));
        Assert.True(list.GetProperty("data").GetArrayLength() >= 1);
    }

    [Fact]
    public async Task Upsert_requires_teacher()
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("terms"), "type" },
            { new StringContent("x"), "content" }
        };
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().PutAsync("/api/privacy-policy", form)).StatusCode);
    }

    [Fact]
    public async Task Unknown_type_returns_not_found()
        => Assert.Equal(HttpStatusCode.NotFound, (await factory.CreateClient().GetAsync("/api/privacy-policy/does-not-exist")).StatusCode);
}
