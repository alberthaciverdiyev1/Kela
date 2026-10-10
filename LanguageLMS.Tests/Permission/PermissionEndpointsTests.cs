using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Permission;

[Collection("api")]
public class PermissionEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static MultipartFormDataContent SubjectForm() => new() { { new StringContent(Unique("Subj")), "name" } };

    private async Task<string> TeacherTokenAsync()
    {
        var email = $"{Unique("t")}@ex.com";
        var form = new MultipartFormDataContent
        {
            { new StringContent("T"), "name" }, { new StringContent(email), "email" },
            { new StringContent("teacher"), "type" }, { new StringContent("secret123"), "password" }
        };
        await _admin.PostAsync("/api/user", form);
        var login = await _admin.PostAsJsonAsync("/api/user-auth/login", new { email, password = "secret123" });
        return (await Json(login)).GetProperty("data").GetProperty("token").GetString()!;
    }

    private static HttpClient Authed(ApiFactory factory, string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    [Fact]
    public async Task Teacher_permission_can_be_granted_and_revoked()
    {
        const string perm = "subject.manage";
        const string other = "comment.manage";
        await _admin.PostAsJsonAsync("/api/permission", new { name = perm });
        await _admin.PostAsJsonAsync("/api/permission", new { name = other });

        var teacher = Authed(factory, await TeacherTokenAsync());

        try
        {
            // verilib → subject yarada bilər
            await _admin.PutAsJsonAsync("/api/permission/role", new { role = "teacher", permissions = new[] { perm } });
            Assert.Equal(HttpStatusCode.Created, (await teacher.PostAsync("/api/subject", SubjectForm())).StatusCode);

            // başqa icazə verilib, bu yox → 403
            await _admin.PutAsJsonAsync("/api/permission/role", new { role = "teacher", permissions = new[] { other } });
            Assert.Equal(HttpStatusCode.Forbidden, (await teacher.PostAsync("/api/subject", SubjectForm())).StatusCode);
        }
        finally
        {
            // qlobal vəziyyəti sıfırla (digər testlərə təsir etməsin)
            await _admin.PutAsJsonAsync("/api/permission/role", new { role = "teacher", permissions = Array.Empty<string>() });
        }
    }

    [Fact]
    public async Task Admin_bypasses_permissions()
        => Assert.Equal(HttpStatusCode.Created, (await _admin.PostAsync("/api/subject", SubjectForm())).StatusCode);

    [Fact]
    public async Task List_requires_admin()
    {
        var teacher = Authed(factory, await TeacherTokenAsync());
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.GetAsync("/api/permission")).StatusCode);
    }
}
