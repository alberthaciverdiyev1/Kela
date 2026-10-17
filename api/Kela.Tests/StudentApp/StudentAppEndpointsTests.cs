using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kela.Tests.StudentApp;

[Collection("api")]
public class StudentAppEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement json)
        => json.GetProperty("data").EnumerateArray();

    private async Task<(long id, string token)> RegisterStudentAsync()
    {
        var r = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Student",
            email = $"student-{Guid.NewGuid():N}@example.com",
            password = "secret123"
        });
        var data = (await ReadJsonAsync(r)).GetProperty("data");
        return (data.GetProperty("user").GetProperty("id").GetInt64(), data.GetProperty("token").GetString()!);
    }

    private async Task<HttpClient> StudentClientAsync(string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    [Fact]
    public async Task Student_sees_assigned_content_chain()
    {
        var (studentId, token) = await RegisterStudentAsync();
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subject"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("Lesson"));
        var groupId = await TestData.GroupAsync(factory, Unique("Group"));

        await TestData.AssignSubjectsAsync(factory, groupId, subjectId);
        await TestData.AssignStudentsAsync(factory, groupId, new[] { studentId });

        var student = await StudentClientAsync(token);

        var groups = await ReadJsonAsync(await student.GetAsync("/api/student/groups"));
        Assert.Contains(Items(groups), x => x.GetProperty("id").GetInt64() == groupId);

        var subjects = await ReadJsonAsync(await student.GetAsync($"/api/student/groups/{groupId}/subjects"));
        Assert.Contains(Items(subjects), x => x.GetProperty("id").GetInt64() == subjectId);

        var lessons = await ReadJsonAsync(await student.GetAsync($"/api/student/subjects/{subjectId}/lessons"));
        Assert.Contains(Items(lessons), x => x.GetProperty("id").GetInt64() == lessonId);

        var contents = await student.GetAsync($"/api/student/lessons/{lessonId}/contents");
        Assert.Equal(HttpStatusCode.OK, contents.StatusCode);
    }

    [Fact]
    public async Task Student_of_other_group_gets_forbidden()
    {
        var (_, token) = await RegisterStudentAsync();
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subject"));
        var groupId = await TestData.GroupAsync(factory, Unique("Group"));
        await TestData.AssignSubjectsAsync(factory, groupId, subjectId);

        var student = await StudentClientAsync(token);
        var response = await student.GetAsync($"/api/student/groups/{groupId}/subjects");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Student_endpoints_require_auth()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/student/groups")).StatusCode);

    [Fact]
    public async Task Non_student_token_cannot_access_student_endpoints()
    {
        // admin (teacher) tokeni şagird uclarına girə bilməməlidir
        var admin = await StudentClientAsync(factory.AdminToken);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.GetAsync("/api/student/groups")).StatusCode);
    }
}
