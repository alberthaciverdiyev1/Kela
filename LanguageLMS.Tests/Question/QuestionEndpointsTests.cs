using System.Net;
using System.Text.Json;

namespace LanguageLMS.Tests.Question;

[Collection("api")]
public class QuestionEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement json)
        => json.GetProperty("data").GetProperty("data").EnumerateArray();

    private async Task<long> CreateSubjectAsync()
    {
        var r = await _client.PostAsync("/api/subject", new MultipartFormDataContent { { new StringContent(Unique("Subject")), "name" } });
        return (await ReadJsonAsync(r)).GetProperty("data").GetProperty("id").GetInt64();
    }

    private MultipartFormDataContent Form(long subjectId, string title, string correct = "A")
        => new()
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent(title), "title" },
            { new StringContent("one"), "variant_a" },
            { new StringContent("two"), "variant_b" },
            { new StringContent("three"), "variant_c" },
            { new StringContent("four"), "variant_d" },
            { new StringContent(correct), "correct_answer" }
        };

    [Fact]
    public async Task Full_crud_flow()
    {
        var subjectId = await CreateSubjectAsync();
        var title = Unique("Q");
        var create = await _client.PostAsync("/api/question", Form(subjectId, title, "C"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await ReadJsonAsync(create);
        var id = created.GetProperty("data").GetProperty("id").GetInt64();
        Assert.Equal("C", created.GetProperty("data").GetProperty("correctAnswer").GetString());

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/question?subject_id={subjectId}&search={title}"));
        Assert.Contains(Items(list), x => x.GetProperty("id").GetInt64() == id);

        var update = await _client.PutAsync($"/api/question/{id}", Form(subjectId, title, "B"));
        Assert.Equal("B", (await ReadJsonAsync(update)).GetProperty("data").GetProperty("correctAnswer").GetString());

        Assert.Equal(HttpStatusCode.OK, (await _client.DeleteAsync($"/api/question/{id}")).StatusCode);
    }

    [Fact]
    public async Task Invalid_correct_answer_returns_validation_error()
    {
        var subjectId = await CreateSubjectAsync();
        var response = await _client.PostAsync("/api/question", Form(subjectId, Unique("Q"), "Z"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_with_unknown_subject_returns_bad_request()
    {
        var response = await _client.PostAsync("/api/question", Form(999999, Unique("Q")));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Question_supports_image_title()
    {
        var subjectId = await CreateSubjectAsync();
        var form = new MultipartFormDataContent
        {
            { new StringContent(subjectId.ToString()), "subject_id" },
            { new StringContent("image question"), "title" },
            { new StringContent("a"), "variant_a" }, { new StringContent("b"), "variant_b" },
            { new StringContent("c"), "variant_c" }, { new StringContent("d"), "variant_d" },
            { new StringContent("A"), "correct_answer" },
            { new StringContent("image"), "title_type" }
        };
        var file = new ByteArrayContent(new byte[10]);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        form.Add(file, "title_image", "title.png");

        var response = await _client.PostAsync("/api/question", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var data = (await ReadJsonAsync(response)).GetProperty("data");
        Assert.Equal("image", data.GetProperty("titleType").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("titleUrl").GetString()));
    }
}
