using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Admin;

[Collection("api")]
public class MiscAdminEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement j) => j.GetProperty("data").GetProperty("data").EnumerateArray();

    private async Task<long> SubjectAsync()
        => (await Json(await _admin.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("S")), "name" } }))).GetProperty("data").GetProperty("id").GetInt64();

    private async Task<long> LessonAsync(long subjectId, string title)
        => (await Json(await _admin.PostAsync("/api/lesson", new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" }, { new StringContent(title), "title" }
        }))).GetProperty("data").GetProperty("id").GetInt64();

    [Fact]
    public async Task Reorder_lessons()
    {
        var subjectId = await SubjectAsync();
        var l1 = await LessonAsync(subjectId, Unique("L1"));
        var l2 = await LessonAsync(subjectId, Unique("L2"));

        var sort = await _admin.PostAsJsonAsync("/api/lesson/sort", new { ids = new[] { l2, l1 } });
        Assert.Equal(HttpStatusCode.OK, sort.StatusCode);

        var list = await Json(await _admin.GetAsync($"/api/lesson?subject_id={subjectId}&per_page=200"));
        var ordered = Items(list).Where(x => x.GetProperty("id").GetInt64() == l2 || x.GetProperty("id").GetInt64() == l1)
            .OrderBy(x => x.GetProperty("sortOrder").GetInt32()).Select(x => x.GetProperty("id").GetInt64()).ToList();
        Assert.Equal(new[] { l2, l1 }, ordered);
    }

    [Fact]
    public async Task Editor_upload_and_cache_clear()
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(new byte[10]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "file", "img.png");
        var upload = await _admin.PostAsync("/api/editor/upload", form);
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace((await Json(upload)).GetProperty("data").GetProperty("url").GetString()));

        Assert.Equal(HttpStatusCode.OK, (await _admin.PostAsync("/api/cache/clear", null)).StatusCode);
    }

    [Fact]
    public async Task Lesson_settings_get_and_set()
    {
        var lessonId = await LessonAsync(await SubjectAsync(), Unique("L"));

        var set = await _admin.PutAsJsonAsync($"/api/lesson/{lessonId}/settings", new { topicExplain = 0, topicQuiz = 0 });
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);
        Assert.Equal(0, (await Json(set)).GetProperty("data").GetProperty("topicExplain").GetInt32());

        var get = await Json(await _admin.GetAsync($"/api/lesson/{lessonId}/settings"));
        Assert.Equal(0, get.GetProperty("data").GetProperty("topicQuiz").GetInt32());
        Assert.Equal(1, get.GetProperty("data").GetProperty("pdf").GetInt32());
    }
}
