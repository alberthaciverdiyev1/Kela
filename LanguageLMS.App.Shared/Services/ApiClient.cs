using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanguageLMS.App.Shared.Services;

/// <summary>API çağırışları: `{success, data, message}` zərfini açır.</summary>
public sealed class ApiClient(HttpClient http, AuthState auth, TokenStore store)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string? Token { get; set; }

    private void ApplyAuth()
    {
        http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(Token)
            ? null
            : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
    }

    public Task<T> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path, null);
    public Task<T> PostAsync<T>(string path, object? body = null) => SendAsync<T>(HttpMethod.Post, path, body);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body)
    {
        ApplyAuth();
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Şəbəkə xətası: {ex.Message}");
        }

        // Token vaxtı dolub / auth yoxdur → sessiyanı təmizlə (MainLayout login göstərəcək)
        if (response.StatusCode == HttpStatusCode.Unauthorized && auth.IsAuthenticated)
        {
            await SignOutAsync();
            throw new ApiException("Sessiya bitib. Yenidən daxil olun.", 401);
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
            throw new ApiException(message ?? $"Xəta ({(int)response.StatusCode})", (int)response.StatusCode);
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
