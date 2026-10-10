using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Group;

[Collection("api")]
public class GroupEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static MultipartFormDataContent Form(string name, string? description = null)
    {
        var form = new MultipartFormDataContent { { new StringContent(name), "name" } };
        if (description is not null) form.Add(new StringContent(description), "description");
        return form;
    }

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<long> CreateGroupAsync(string name)
    {
        var response = await _client.PostAsync("/api/group", Form(name));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<long> CreateStudentAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Student One",
            email = $"student-{Guid.NewGuid():N}@example.com",
            password = "secret123"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("data").GetProperty("user").GetProperty("id").GetInt64();
    }

    [Fact]
    public async Task Full_crud_flow()
    {
        var name = Unique("Group");
        var id = await CreateGroupAsync(name);

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/group?search={name}"));
        Assert.Contains(list.GetProperty("data").GetProperty("data").EnumerateArray(),
            x => x.GetProperty("id").GetInt64() == id);

        var show = await ReadJsonAsync(await _client.GetAsync($"/api/group/{id}"));
        Assert.Equal(name, show.GetProperty("data").GetProperty("name").GetString());

        var update = await _client.PutAsync($"/api/group/{id}", Form(name, "updated desc"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await _client.DeleteAsync($"/api/group/{id}")).StatusCode);
    }

    [Fact]
    public async Task Assign_list_and_remove_students()
    {
        var groupId = await CreateGroupAsync(Unique("Group-students"));
        var studentId = await CreateStudentAsync();

        var assign = await _client.PostAsJsonAsync($"/api/group/{groupId}/students", new
        {
            studentIds = new[] { studentId },
            startDate = "2026-01-01",
            endDate = "2026-12-31"
        });
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);
        var assigned = await ReadJsonAsync(assign);
        Assert.Contains(assigned.GetProperty("data").EnumerateArray(),
            x => x.GetProperty("studentId").GetInt64() == studentId);

        var members = await ReadJsonAsync(await _client.GetAsync($"/api/group/{groupId}/students"));
        Assert.Contains(members.GetProperty("data").EnumerateArray(),
            x => x.GetProperty("studentId").GetInt64() == studentId);

        var remove = await _client.DeleteAsync($"/api/group/{groupId}/students/{studentId}");
        Assert.Equal(HttpStatusCode.OK, remove.StatusCode);

        var after = await ReadJsonAsync(await _client.GetAsync($"/api/group/{groupId}/students"));
        Assert.DoesNotContain(after.GetProperty("data").EnumerateArray(),
            x => x.GetProperty("studentId").GetInt64() == studentId);
    }

    [Fact]
    public async Task Assign_unknown_student_returns_bad_request()
    {
        var groupId = await CreateGroupAsync(Unique("Group-bad"));
        var response = await _client.PostAsJsonAsync($"/api/group/{groupId}/students", new { studentIds = new[] { 999999L } });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<long> CreateSubjectAsync()
    {
        var response = await _client.PostAsync("/api/subject", new MultipartFormDataContent
        {
            { new StringContent(Unique("Subject")), "name" }
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("data").GetProperty("id").GetInt64();
    }

    [Fact]
    public async Task Assign_list_and_remove_subjects()
    {
        var groupId = await CreateGroupAsync(Unique("Group-subjects"));
        var subjectId = await CreateSubjectAsync();

        var assign = await _client.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { subjectId } });
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);
        Assert.Contains((await ReadJsonAsync(assign)).GetProperty("data").EnumerateArray(),
            x => x.GetProperty("id").GetInt64() == subjectId);

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/group/{groupId}/subjects"));
        Assert.Contains(list.GetProperty("data").EnumerateArray(),
            x => x.GetProperty("id").GetInt64() == subjectId);

        var remove = await _client.DeleteAsync($"/api/group/{groupId}/subjects/{subjectId}");
        Assert.Equal(HttpStatusCode.OK, remove.StatusCode);

        var after = await ReadJsonAsync(await _client.GetAsync($"/api/group/{groupId}/subjects"));
        Assert.DoesNotContain(after.GetProperty("data").EnumerateArray(),
            x => x.GetProperty("id").GetInt64() == subjectId);
    }

    [Fact]
    public async Task Assign_unknown_subject_returns_bad_request()
    {
        var groupId = await CreateGroupAsync(Unique("Group-badsubject"));
        var response = await _client.PostAsJsonAsync($"/api/group/{groupId}/subjects", new { subjectIds = new[] { 999999L } });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_id_returns_not_found()
        => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/group/999999")).StatusCode);
}
