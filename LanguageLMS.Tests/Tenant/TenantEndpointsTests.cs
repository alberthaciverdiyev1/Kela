using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.Tests.Tenant;

[Collection("api")]
public class TenantEndpointsTests(ApiFactory factory)
{
    private readonly HttpClient _admin = factory.TeacherClient();

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> Json(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<long> CreateTenantAsync()
    {
        var client = factory.CreateClient();
        var r = await client.PostAsJsonAsync("/api/tenant", new { name = "Tenant", slug = Unique("t").ToLower() });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await Json(r)).GetProperty("data").GetProperty("id").GetInt64();
    }

    private async Task<HttpResponseMessage> PostSubjectAsync(long tenantId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/subject")
        {
            Content = new MultipartFormDataContent { { new StringContent(Unique("Subj")), "name" } }
        };
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await _admin.SendAsync(request);
    }

    private async Task<HttpResponseMessage> ListSubjectsAsync(long? tenantId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/subject?per_page=200");
        if (tenantId.HasValue) request.Headers.Add("X-Tenant-Id", tenantId.Value.ToString());
        return await _admin.SendAsync(request);
    }

    private static IEnumerable<JsonElement> Items(JsonElement json)
        => json.GetProperty("data").GetProperty("data").EnumerateArray();

    [Fact]
    public async Task Tenants_are_isolated()
    {
        var t1 = await CreateTenantAsync();
        var t2 = await CreateTenantAsync();

        var name = Unique("Subj");
        var create = new HttpRequestMessage(HttpMethod.Post, "/api/subject")
        {
            Content = new MultipartFormDataContent { { new StringContent(name), "name" } }
        };
        create.Headers.Add("X-Tenant-Id", t1.ToString());
        Assert.Equal(HttpStatusCode.Created, (await _admin.SendAsync(create)).StatusCode);

        var inT1 = await Json(await ListSubjectsAsync(t1));
        Assert.Contains(Items(inT1), x => x.GetProperty("name").GetString() == name);

        var inT2 = await Json(await ListSubjectsAsync(t2));
        Assert.DoesNotContain(Items(inT2), x => x.GetProperty("name").GetString() == name);
    }

    [Fact]
    public async Task Create_and_list_tenants()
    {
        var id = await CreateTenantAsync();
        var list = await Json(await _admin.GetAsync("/api/tenant"));
        Assert.Contains(list.GetProperty("data").EnumerateArray(), x => x.GetProperty("id").GetInt64() == id);
    }

    [Fact]
    public async Task Duplicate_slug_returns_bad_request()
    {
        var client = factory.CreateClient();
        var slug = Unique("slug").ToLower();
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/tenant", new { name = "A", slug })).StatusCode);

        var second = await client.PostAsJsonAsync("/api/tenant", new { name = "B", slug });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }
}
