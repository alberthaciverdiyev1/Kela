using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Admin;

[Collection("api")]
public class CommitBEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    [Fact]
    public async Task System_notification_create_read()
    {
        // admin özünə bildiriş yaradır (userId = admin token-dəki NameIdentifier; bilmədiyimiz üçün özünü hədəfləyirik)
        // əvvəl users list-dən öz id-ni tapmaq əvəzinə, create sonrası list yoxlayırıq.
        var me = await Json(await _admin.GetAsync("/api/user-auth/me"));
        var myId = me.GetProperty("data").GetProperty("id").GetInt64();

        var create = await _admin.PostAsJsonAsync("/api/system-notification", new { recipientId = myId, title = Unique("Bildiriş") });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = (await Json(create)).GetProperty("data").GetProperty("id").GetInt64();

        var list = await Json(await _admin.GetAsync("/api/system-notification"));
        Assert.Contains(list.GetProperty("data").EnumerateArray(), x => x.GetProperty("id").GetInt64() == id);

        Assert.Equal(HttpStatusCode.OK, (await _admin.PostAsync($"/api/system-notification/{id}/read", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _admin.PostAsync("/api/system-notification/read-all", null)).StatusCode);
    }

    [Fact]
    public async Task Subject_assignment_writes_audit_log()
    {
        var subjectId = (await Json(await _admin.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("S")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();
        var groupId = (await Json(await _admin.PostAsync("/api/group", new MultipartFormDataContent { { new StringContent(Unique("G")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();

        await _admin.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { subjectId } });

        var logs = await Json(await _admin.GetAsync($"/api/group/{groupId}/subject-logs"));
        Assert.Contains(logs.GetProperty("data").EnumerateArray(),
            x => x.GetProperty("subjectId").GetInt64() == subjectId && x.GetProperty("action").GetString() == "attach");

        await _admin.DeleteAsync($"/api/group/{groupId}/subjects/{subjectId}");
        var logs2 = await Json(await _admin.GetAsync($"/api/group/{groupId}/subject-logs"));
        Assert.Contains(logs2.GetProperty("data").EnumerateArray(), x => x.GetProperty("action").GetString() == "detach");
    }
}
