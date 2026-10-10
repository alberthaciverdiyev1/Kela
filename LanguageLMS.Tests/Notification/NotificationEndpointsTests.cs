using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Notification;

[Collection("api")]
public class NotificationEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<string> TeacherTokenAsync()
    {
        var email = $"{Unique("t")}@ex.com";
        var form = new MultipartFormDataContent
        {
            { new StringContent("T"), "name" }, { new StringContent(email), "email" },
            { new StringContent("teacher"), "type" }, { new StringContent("secret123"), "password" }
        };
        await _client.PostAsync("/api/user", form);
        var login = await _client.PostAsJsonAsync("/api/user-auth/login", new { email, password = "secret123" });
        return (await Json(login)).GetProperty("data").GetProperty("token").GetString()!;
    }

    private async Task<(long id, string token)> RegisterStudentAsync()
    {
        var r = await _client.PostAsJsonAsync("/api/auth/register", new { fullName = "S", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var d = (await Json(r)).GetProperty("data");
        return (d.GetProperty("user").GetProperty("id").GetInt64(), d.GetProperty("token").GetString()!);
    }

    private static HttpClient Authed(ApiFactory factory, string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    private static MultipartFormDataContent Form(string title, bool all = true, IEnumerable<long>? studentIds = null)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(title), "title" },
            { new StringContent("body text"), "body" },
            { new StringContent(all ? "true" : "false"), "all" }
        };
        if (studentIds is not null)
        {
            var i = 0;
            foreach (var id in studentIds) form.Add(new StringContent(id.ToString()), $"studentIds[{i++}]");
        }
        return form;
    }

    [Fact]
    public async Task Broadcast_notification_visible_to_student()
    {
        var teacher = Authed(factory, await TeacherTokenAsync());
        var (_, token) = await RegisterStudentAsync();
        var student = Authed(factory, token);

        var title = Unique("Broadcast");
        var create = await teacher.PostAsync("/api/notification", Form(title));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var list = await Json(await student.GetAsync("/api/student/notifications"));
        Assert.Contains(list.GetProperty("data").GetProperty("data").EnumerateArray(),
            x => x.GetProperty("title").GetString() == title);
    }

    [Fact]
    public async Task Targeted_notification_only_visible_to_target()
    {
        var teacher = Authed(factory, await TeacherTokenAsync());
        var (targetId, targetToken) = await RegisterStudentAsync();
        var (_, otherToken) = await RegisterStudentAsync();

        var title = Unique("Targeted");
        var create = await teacher.PostAsync("/api/notification", Form(title, all: false, studentIds: new[] { targetId }));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var target = Authed(factory, targetToken);
        var targetList = await Json(await target.GetAsync("/api/student/notifications"));
        Assert.Contains(targetList.GetProperty("data").GetProperty("data").EnumerateArray(),
            x => x.GetProperty("title").GetString() == title);

        var other = Authed(factory, otherToken);
        var otherList = await Json(await other.GetAsync("/api/student/notifications"));
        Assert.DoesNotContain(otherList.GetProperty("data").GetProperty("data").EnumerateArray(),
            x => x.GetProperty("title").GetString() == title);
    }

    [Fact]
    public async Task Student_can_save_and_remove_token()
    {
        var (_, token) = await RegisterStudentAsync();
        var student = Authed(factory, token);
        var deviceToken = $"tok-{Guid.NewGuid():N}";

        var save = await student.PostAsJsonAsync("/api/student/notifications/token", new { token = deviceToken, deviceType = "android" });
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);

        var remove = await student.DeleteAsync($"/api/student/notifications/token?token={deviceToken}");
        Assert.Equal(HttpStatusCode.OK, remove.StatusCode);
    }

    [Fact]
    public async Task Notification_requires_teacher_auth()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/notification")).StatusCode);
}
