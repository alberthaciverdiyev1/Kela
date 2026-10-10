using System.Net;
using System.Text.Json;

namespace LanguageLMS.Tests.User;

[Collection("api")]
public class UserEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string Email() => $"teacher-{Guid.NewGuid():N}@example.com";

    private static MultipartFormDataContent Form(string email, string type = "teacher", string? password = "secret123")
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("Ali"), "name" },
            { new StringContent("Veli"), "surname" },
            { new StringContent(email), "email" },
            { new StringContent(type), "type" }
        };
        if (password is not null) form.Add(new StringContent(password), "password");
        return form;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    [Fact]
    public async Task Full_crud_flow()
    {
        var email = Email();
        var create = await _client.PostAsync("/api/user", Form(email));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await ReadJsonAsync(create);
        var id = created.GetProperty("data").GetProperty("id").GetInt64();
        Assert.Equal("teacher", created.GetProperty("data").GetProperty("type").GetString());

        var list = await ReadJsonAsync(await _client.GetAsync($"/api/user?search={email}"));
        Assert.Contains(list.GetProperty("data").GetProperty("data").EnumerateArray(),
            x => x.GetProperty("id").GetInt64() == id);

        // şifresiz güncelleme mevcut şifreyi korur
        var update = await _client.PutAsync($"/api/user/{id}", Form(email, password: null));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await _client.DeleteAsync($"/api/user/{id}")).StatusCode);
    }

    [Fact]
    public async Task Duplicate_email_returns_bad_request()
    {
        var email = Email();
        await _client.PostAsync("/api/user", Form(email));

        var second = await _client.PostAsync("/api/user", Form(email));
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Create_without_password_returns_bad_request()
    {
        var response = await _client.PostAsync("/api/user", Form(Email(), password: null));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_id_returns_not_found()
        => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/user/999999")).StatusCode);
}
