using System.Net;
using System.Text.Json;

namespace LanguageLMS.Tests.Profession;

[Collection("api")]
public class ProfessionEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static MultipartFormDataContent Form(string name, long? parentId = null)
    {
        var form = new MultipartFormDataContent { { new StringContent(name), "name" } };
        if (parentId.HasValue) form.Add(new StringContent(parentId.Value.ToString()), "parent_id");
        return form;
    }

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
        => (await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync())).RootElement.Clone();

    private static async Task<long> CreateAsync(HttpClient client, string name, long? parentId = null)
    {
        var response = await client.PostAsync("/api/profession", Form(name, parentId));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("data").GetProperty("id").GetInt64();
    }

    [Fact]
    public async Task Full_crud_flow()
    {
        var name = Unique("Prof");
        var id = await CreateAsync(_client, name);

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/profession?search={name}"));
        Assert.Contains(list.GetProperty("data").GetProperty("data").EnumerateArray(),
            x => x.GetProperty("id").GetInt64() == id);

        var show = await ReadJsonAsync(await _client.GetAsync($"/api/profession/{id}"));
        Assert.Equal(name, show.GetProperty("data").GetProperty("name").GetString());

        var updated = Unique("Prof-upd");
        var update = await _client.PutAsync($"/api/profession/{id}", Form(updated));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(updated, (await ReadJsonAsync(update)).GetProperty("data").GetProperty("name").GetString());

        Assert.Equal(HttpStatusCode.OK, (await _client.DeleteAsync($"/api/profession/{id}")).StatusCode);
    }

    [Fact]
    public async Task List_returns_sub_categories_of_parent()
    {
        var parentId = await CreateAsync(_client, Unique("Parent"));
        var childName = Unique("Child");
        var childId = await CreateAsync(_client, childName, parentId);

        var show = await ReadJsonAsync(await _client.GetAsync($"/api/profession/{parentId}"));
        var subs = show.GetProperty("data").GetProperty("subCategories").EnumerateArray();
        Assert.Contains(subs, x => x.GetProperty("id").GetInt64() == childId);
    }

    [Fact]
    public async Task Duplicate_name_returns_bad_request()
    {
        var name = Unique("Dup");
        await CreateAsync(_client, name);

        var second = await _client.PostAsync("/api/profession", Form(name));
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.False((await ReadJsonAsync(second)).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Parent_must_be_top_level()
    {
        var parentId = await CreateAsync(_client, Unique("Top"));
        var childId = await CreateAsync(_client, Unique("Sub"), parentId);

        // child (alt ixtisas) başka bir kategorinin parent'ı olamaz
        var response = await _client.PostAsync("/api/profession", Form(Unique("Grand"), childId));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_delete_profession_with_children()
    {
        var parentId = await CreateAsync(_client, Unique("WithKids"));
        await CreateAsync(_client, Unique("Kid"), parentId);

        var response = await _client.DeleteAsync($"/api/profession/{parentId}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
