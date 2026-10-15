using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kela.App.Shared.Services.Localization;

namespace Kela.App.Shared.Services;

/// <summary>API çağırışları: `{success, data, message}` zərfini açır.</summary>
public sealed class ApiClient(HttpClient http, AuthState auth, TokenStore store, Localizer lang)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string? Token { get; set; }

    /// <summary>Seçilmiş tenant — hər sorğuda X-Tenant-Id header-ı kimi göndərilir.</summary>
    public long? TenantId { get; set; }

    private void ApplyAuth()
    {
        http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", Token);

        http.DefaultRequestHeaders.Remove("X-Tenant-Id");
        if (TenantId is > 0)
        {
            http.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId.Value.ToString());
        }

        http.DefaultRequestHeaders.AcceptLanguage.Clear();
        http.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(lang.Current));
    }

    public Task<T> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path, null);
    public Task<T> PostAsync<T>(string path, object? body = null) => SendAsync<T>(HttpMethod.Post, path, body);
    public Task<T> PutAsync<T>(string path, object? body = null) => SendAsync<T>(HttpMethod.Put, path, body);
    public Task<T> DeleteAsync<T>(string path) => SendAsync<T>(HttpMethod.Delete, path, null);

    /// <summary>Multipart fayl yükləmə (məs. profil şəkli).</summary>
    public Task<T> PostFileAsync<T>(string path, Stream content, string fileName, string contentType)
    {
        var form = new MultipartFormDataContent();
        var stream = new StreamContent(content);
        stream.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        form.Add(stream, "file", fileName);
        return SendRequestAsync<T>(HttpMethod.Post, path, form);
    }

    private Task<T> SendAsync<T>(HttpMethod method, string path, object? body)
    {
        HttpContent? content = body is null ? null : JsonContent.Create(body, options: JsonOptions);
        return SendRequestAsync<T>(method, path, content);
    }

    private async Task<T> SendRequestAsync<T>(HttpMethod method, string path, HttpContent? content)
    {
        ApplyAuth();
        using var request = new HttpRequestMessage(method, path) { Content = content };

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request);
        }
        catch (Exception ex)
        {
            throw new ApiException(lang.Format("Error_Network", ex.Message));
        }

        // Token vaxtı dolub / auth yoxdur → sessiyanı təmizlə
        if (response.StatusCode == HttpStatusCode.Unauthorized && auth.IsAuthenticated)
        {
            await SignOutAsync();
            throw new ApiException(lang["Error_SessionExpired"], 401);
        }

        var payload = await response.Content.ReadAsStringAsync();
        JsonElement root = default;
        if (!string.IsNullOrWhiteSpace(payload))
        {
            try { root = JsonDocument.Parse(payload).RootElement; } catch { /* qeyri-JSON */ }
        }

        var success = root.ValueKind == JsonValueKind.Object
                      && root.TryGetProperty("success", out var s) && s.ValueKind == JsonValueKind.True;

        if (!response.IsSuccessStatusCode || !success)
        {
            var message = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("message", out var m)
                ? m.GetString()
                : null;
            throw new ApiException(message ?? lang.Format("Error_Status", (int)response.StatusCode), (int)response.StatusCode);
        }

        if (root.TryGetProperty("data", out var data))
        {
            return data.Deserialize<T>(JsonOptions)!;
        }

        return default!;
    }

    private async Task SignOutAsync()
    {
        Token = null;
        auth.Clear();
        try { await store.RemoveAsync(); } catch { /* JS hazır deyil */ }
    }
}
