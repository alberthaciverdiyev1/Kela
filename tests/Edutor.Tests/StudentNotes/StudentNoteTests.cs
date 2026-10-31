using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Edutor.Tests.StudentNotes;

[Collection("api")]
public class StudentNoteTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

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

    private HttpClient Authed(string token)
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    private async Task<long> ArrangeLessonAsync(params long[] studentIds)
    {
        var subjectId = await TestData.SubjectAsync(factory, Unique("Subject"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("Lesson"));
        var contentId = await TestData.LessonContentAsync(factory, lessonId, Unique("Content"));

        if (studentIds.Length > 0)
        {
            var groupId = await TestData.GroupAsync(factory, Unique("Group"));
            await TestData.AssignSubjectsAsync(factory, groupId, subjectId);
            await TestData.AssignStudentsAsync(factory, groupId, studentIds);
        }

        return contentId;
    }

    [Fact]
    public async Task Private_note_is_visible_only_to_its_author()
    {
        var (studentA, tokenA) = await RegisterStudentAsync();
        var (studentB, tokenB) = await RegisterStudentAsync();
        var contentId = await ArrangeLessonAsync(studentA, studentB);

        var a = Authed(tokenA);
        var create = await a.PostAsJsonAsync("/api/student-note", new
        {
            contentId,
            positionSeconds = 42,
            title = "Intro",
            body = "Remember this",
            visibility = "private"
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        var mine = await ReadJsonAsync(await a.GetAsync($"/api/student-note?content_id={contentId}"));
        var note = Assert.Single(mine.GetProperty("data").EnumerateArray().ToList());
        Assert.True(note.GetProperty("isMine").GetBoolean());
        Assert.Equal(42, note.GetProperty("positionSeconds").GetInt32());

        var other = Authed(tokenB);
        var theirs = await ReadJsonAsync(await other.GetAsync($"/api/student-note?content_id={contentId}"));
        Assert.Empty(theirs.GetProperty("data").EnumerateArray().ToList());
    }

    [Fact]
    public async Task Group_note_is_visible_to_group_members_only()
    {
        var (studentA, tokenA) = await RegisterStudentAsync();
        var (studentB, tokenB) = await RegisterStudentAsync();
        var (studentC, tokenC) = await RegisterStudentAsync();

        var subjectId = await TestData.SubjectAsync(factory, Unique("Subject"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, Unique("Lesson"));
        var contentId = await TestData.LessonContentAsync(factory, lessonId, Unique("Content"));

        var sharedGroup = await TestData.GroupAsync(factory, Unique("Group"));
        await TestData.AssignSubjectsAsync(factory, sharedGroup, subjectId);
        await TestData.AssignStudentsAsync(factory, sharedGroup, new[] { studentA, studentB });

        var otherGroup = await TestData.GroupAsync(factory, Unique("Other"));
        await TestData.AssignSubjectsAsync(factory, otherGroup, subjectId);
        await TestData.AssignStudentsAsync(factory, otherGroup, new[] { studentC });

        var a = Authed(tokenA);
        var create = await a.PostAsJsonAsync("/api/student-note", new
        {
            contentId,
            body = "Shared with my group",
            visibility = "group",
            groupId = sharedGroup
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        var member = await ReadJsonAsync(await Authed(tokenB).GetAsync($"/api/student-note?content_id={contentId}"));
        var sharedNote = Assert.Single(member.GetProperty("data").EnumerateArray().ToList());
        Assert.False(sharedNote.GetProperty("isMine").GetBoolean());
        Assert.Equal("group", sharedNote.GetProperty("visibility").GetString());

        var outsider = await ReadJsonAsync(await Authed(tokenC).GetAsync($"/api/student-note?content_id={contentId}"));
        Assert.Empty(outsider.GetProperty("data").EnumerateArray().ToList());
    }

    [Fact]
    public async Task Student_can_create_free_note_without_material()
    {
        var (_, token) = await RegisterStudentAsync();

        var student = Authed(token);
        var create = await student.PostAsJsonAsync("/api/student-note", new
        {
            body = "Independent idea",
            visibility = "private"
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        var free = await ReadJsonAsync(await student.GetAsync("/api/student-note?only_free=true"));
        var note = Assert.Single(free.GetProperty("data").EnumerateArray().ToList());
        Assert.Equal("Independent idea", note.GetProperty("body").GetString());

        var withMaterial = await ReadJsonAsync(await student.GetAsync("/api/student-note"));
        Assert.Single(withMaterial.GetProperty("data").EnumerateArray().ToList());
    }

    [Fact]
    public async Task Only_author_can_update_or_delete_a_note()
    {
        var (studentA, tokenA) = await RegisterStudentAsync();
        var (studentB, tokenB) = await RegisterStudentAsync();
        var contentId = await ArrangeLessonAsync(studentA, studentB);

        var a = Authed(tokenA);
        var create = await ReadJsonAsync(await a.PostAsJsonAsync("/api/student-note", new
        {
            contentId,
            body = "Original",
            visibility = "private"
        }));
        var noteId = create.GetProperty("data").GetProperty("id").GetInt64();

        var b = Authed(tokenB);
        var foreignUpdate = await b.PutAsJsonAsync($"/api/student-note/{noteId}", new
        {
            contentId,
            body = "Hijacked",
            visibility = "private"
        });
        Assert.Equal(HttpStatusCode.NotFound, foreignUpdate.StatusCode);

        var foreignDelete = await b.DeleteAsync($"/api/student-note/{noteId}");
        Assert.Equal(HttpStatusCode.NotFound, foreignDelete.StatusCode);

        var update = await a.PutAsJsonAsync($"/api/student-note/{noteId}", new
        {
            contentId,
            body = "Edited",
            visibility = "private"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var afterUpdate = await ReadJsonAsync(await a.GetAsync($"/api/student-note?content_id={contentId}"));
        Assert.Equal("Edited", Assert.Single(afterUpdate.GetProperty("data").EnumerateArray().ToList())
            .GetProperty("body").GetString());

        Assert.Equal(HttpStatusCode.OK, (await a.DeleteAsync($"/api/student-note/{noteId}")).StatusCode);

        var afterDelete = await ReadJsonAsync(await a.GetAsync($"/api/student-note?content_id={contentId}"));
        Assert.Empty(afterDelete.GetProperty("data").EnumerateArray().ToList());
    }

    [Fact]
    public async Task Note_on_inaccessible_material_is_forbidden()
    {
        var (_, token) = await RegisterStudentAsync();
        var contentId = await ArrangeLessonAsync();

        var response = await Authed(token).PostAsJsonAsync("/api/student-note", new
        {
            contentId,
            body = "Sneaky",
            visibility = "private"
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Empty_body_is_rejected()
    {
        var (_, token) = await RegisterStudentAsync();

        var response = await Authed(token).PostAsJsonAsync("/api/student-note", new
        {
            body = "",
            visibility = "private"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
