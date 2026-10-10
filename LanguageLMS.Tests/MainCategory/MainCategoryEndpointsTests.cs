using System.Net;
using System.Text.Json;

namespace LanguageLMS.Tests.MainCategory;

[Collection("api")]
public class MainCategoryEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static MultipartFormDataContent Form(string name)
        => new() { { new StringContent(name), "name" } };

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static IEnumerable<JsonElement> Items(JsonElement json)
        => json.GetProperty("data").GetProperty("data").EnumerateArray();

    [Fact]
    public async Task Full_crud_flow()
    {
        var name = Unique("Fenn");
        var create = await _client.PostAsync("/api/main-category", Form(name));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = (await ReadJsonAsync(create)).GetProperty("data").GetProperty("id").GetInt64();

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/main-category?search={name}"));
        Assert.Contains(Items(list), x => x.GetProperty("id").GetInt64() == id);

        var show = await ReadJsonAsync(await _client.GetAsync($"/api/main-category/{id}"));
        Assert.Equal(name, show.GetProperty("data").GetProperty("name").GetString());

        var updated = Unique("Fenn-upd");
        var update = await _client.PutAsync($"/api/main-category/{id}", Form(updated));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(updated, (await ReadJsonAsync(update)).GetProperty("data").GetProperty("name").GetString());

        Assert.Equal(HttpStatusCode.OK, (await _client.DeleteAsync($"/api/main-category/{id}")).StatusCode);
    }

    [Fact]
    public async Task Duplicate_name_returns_bad_request()
    {
        var name = Unique("DupFenn");
        await _client.PostAsync("/api/main-category", Form(name));

        var second = await _client.PostAsync("/api/main-category", Form(name));
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Unknown_id_returns_not_found()
        => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/main-category/999999")).StatusCode);
}
