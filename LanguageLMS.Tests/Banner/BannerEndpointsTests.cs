using System.Net;
using System.Text.Json;

namespace LanguageLMS.Tests.Banner;

[Collection("api")]
public class BannerEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static MultipartFormDataContent Form(string title)
        => new() { { new StringContent(title), "title" } };

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.Clone();
    }

    private static IEnumerable<JsonElement> Items(JsonElement json)
        => json.GetProperty("data").GetProperty("data").EnumerateArray();

    [Fact]
    public async Task Full_crud_flow()
    {
        var create = await _client.PostAsync("/api/banner", Form("Flow banner"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await ReadJsonAsync(create);
        Assert.True(created.GetProperty("success").GetBoolean());
        var id = created.GetProperty("data").GetProperty("id").GetInt64();
        Assert.Equal("Flow banner", created.GetProperty("data").GetProperty("title").GetString());

        var list = await ReadJsonAsync(await _client.GetAsync("/api/banner"));
        Assert.Contains(Items(list), x => x.GetProperty("id").GetInt64() == id);

        var show = await ReadJsonAsync(await _client.GetAsync($"/api/banner/{id}"));
        Assert.Equal("Flow banner", show.GetProperty("data").GetProperty("title").GetString());

        var update = await ReadJsonAsync(await _client.PutAsync($"/api/banner/{id}", Form("Flow banner updated")));
        Assert.Equal("Flow banner updated", update.GetProperty("data").GetProperty("title").GetString());

        var delete = await _client.DeleteAsync($"/api/banner/{id}");
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        // soft delete: aktif listeden çıkar, is_deleted listesine düşer
        var activeList = await ReadJsonAsync(await _client.GetAsync("/api/banner"));
        Assert.DoesNotContain(Items(activeList), x => x.GetProperty("id").GetInt64() == id);

        var deletedList = await ReadJsonAsync(await _client.GetAsync("/api/banner?is_deleted=1"));
        Assert.Contains(Items(deletedList), x => x.GetProperty("id").GetInt64() == id);
    }

    [Fact]
    public async Task Create_without_title_returns_validation_envelope()
    {
        var response = await _client.PostAsync("/api/banner", Form(""));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await ReadJsonAsync(response);
        Assert.False(json.GetProperty("success").GetBoolean());
        Assert.Equal(400, json.GetProperty("statusCode").GetInt32());
        Assert.True(json.GetProperty("errors").TryGetProperty("Title", out _));
    }

    [Fact]
    public async Task Unknown_id_returns_not_found_envelope()
    {
        var response = await _client.GetAsync("/api/banner/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var json = await ReadJsonAsync(response);
        Assert.False(json.GetProperty("success").GetBoolean());
        Assert.Equal(404, json.GetProperty("statusCode").GetInt32());
    }

    [Fact]
    public async Task Unknown_route_returns_not_found_envelope()
    {
        var response = await _client.GetAsync("/api/does-not-exist");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False((await ReadJsonAsync(response)).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task List_search_filters_by_title()
    {
        await _client.PostAsync("/api/banner", Form("Searchable-ABC"));
        await _client.PostAsync("/api/banner", Form("Something else"));

        var json = await ReadJsonAsync(await _client.GetAsync("/api/banner?search=Searchable-ABC"));
        var data = Items(json).ToList();

        Assert.True(data.Count >= 1);
        foreach (var item in data)
        {
            Assert.Contains("Searchable-ABC", item.GetProperty("title").GetString());
        }
    }
}
