using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Roles;

[Collection("api")]
public class RolesEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static MultipartFormDataContent UserForm(string email, string type)
        => new()
        {
            { new StringContent("U"), "name" }, { new StringContent(email), "email" },
            { new StringContent(type), "type" }, { new StringContent("secret123"), "password" }
        };

    private async Task<string> LoginAsync(string email)
    {
        var login = await _admin.PostAsJsonAsync("/api/user-auth/login", new { email, password = "secret123" });
        return (await Json(login)).GetProperty("data").GetProperty("token").GetString()!;
    }

    private static HttpClient Authed(ApiFactory factory, string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    private static MultipartFormDataContent SubjectForm() => new() { { new StringContent(Unique("Subj")), "name" } };

    [Fact]
    public async Task Admin_can_manage_users_but_teacher_cannot()
    {
        // admin creates a teacher
        Assert.Equal(HttpStatusCode.Created, (await _admin.PostAsync("/api/user", UserForm($"{Unique("t")}@ex.com", "teacher"))).StatusCode);

        var teacherToken = await LoginAsync(await CreateTeacherAndReturnEmailAsync());
        var teacher = Authed(factory, teacherToken);

        // teacher -> /api/user (Admin) => 403
        var forbidden = await teacher.PostAsync("/api/user", UserForm($"{Unique("x")}@ex.com", "teacher"));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    private async Task<string> CreateTeacherAndReturnEmailAsync()
    {
        var email = $"{Unique("t")}@ex.com";
        await _admin.PostAsync("/api/user", UserForm(email, "teacher"));
        return email;
    }

    [Fact]
    public async Task Teacher_can_manage_content_admin_too()
    {
        var email = await CreateTeacherAndReturnEmailAsync();
        var teacher = Authed(factory, await LoginAsync(email));

        Assert.Equal(HttpStatusCode.Created, (await teacher.PostAsync("/api/subject", SubjectForm())).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _admin.PostAsync("/api/subject", SubjectForm())).StatusCode);
    }

    [Fact]
    public async Task Student_cannot_manage_content()
    {
        var reg = await _admin.PostAsJsonAsync("/api/auth/register", new { fullName = "S", email = $"{Unique("s")}@ex.com", password = "secret123" });
        var studentToken = (await Json(reg)).GetProperty("data").GetProperty("token").GetString();
        var student = Authed(factory, studentToken!);

        var response = await student.PostAsync("/api/subject", SubjectForm());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_cannot_manage_content()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().PostAsync("/api/subject", SubjectForm())).StatusCode);
}
