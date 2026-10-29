using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Edutor.Nodes.Shared.Contracts;

namespace Edutor.Nodes.Worker;

/// <summary>API zərfi — bütün uclar eyni formanı qaytarır.</summary>
public sealed record Envelope<T>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("statusCode")] int StatusCode,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("data")] T? Data);

public sealed record CoordinatorResult<T>(HttpStatusCode Status, T? Data, string? Message)
{
    public bool IsOk => Status == HttpStatusCode.OK;

    public bool Unauthorized => Status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
}

/// <summary>
/// Koordinator API-si ilə əlaqə. Token yalnız yaddaşda saxlanılır və hər sorğuda
/// başlıqda göndərilir — heç vaxt URL-ə əlavə olunmur.
/// </summary>
public sealed class CoordinatorClient(HttpClient http, WorkerSettings settings, WorkerLog log)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Nəzarət sorğuları kiçikdir; cavab gəlməzsə gözləmək mənasızdır. Timeout
    /// olmasa server yenidən başlayanda köhnə TCP bağlantısı üzərində sorğu
    /// saatlarla asılı qala bilər.
    /// </summary>
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(60);

    private string _token = settings.AccessToken;

    public string Token => _token;

    public void SetToken(string token) => _token = token;

    private Uri Url(string path) => new($"{settings.ApiUrl.TrimEnd('/')}/api/nodes/{path}");

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try
        {
            using var response = await http.GetAsync(new Uri($"{settings.ApiUrl.TrimEnd('/')}/health"), ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    public Task<CoordinatorResult<NodeRegisterResponse>> RegisterAsync(
        string registrationCode, string name, NodeDeviceInfo device, CancellationToken ct = default)
        => SendAsync<NodeRegisterResponse>(HttpMethod.Post, "register",
            new NodeRegisterRequest { RegistrationCode = registrationCode, Name = name, Device = device },
            authenticated: false, ct);

    public Task<CoordinatorResult<NodeHeartbeatResponse>> HeartbeatAsync(NodeHeartbeatRequest request, CancellationToken ct = default)
        => SendAsync<NodeHeartbeatResponse>(HttpMethod.Post, "heartbeat", request, authenticated: true, ct);

    public Task<CoordinatorResult<NodeRotateTokenResponse>> RotateTokenAsync(CancellationToken ct = default)
        => SendAsync<NodeRotateTokenResponse>(HttpMethod.Post, "token/rotate", new { }, authenticated: true, ct);

    public Task<CoordinatorResult<IReadOnlyList<JobAssignment>>> ClaimAsync(
        int max, long? preferVideoId = null, CancellationToken ct = default)
        => SendAsync<IReadOnlyList<JobAssignment>>(HttpMethod.Post, "jobs/claim",
            new JobClaimRequest { Max = max, PreferVideoId = preferVideoId }, authenticated: true, ct);

    public Task<CoordinatorResult<object>> ProgressAsync(long jobId, JobProgressRequest request, CancellationToken ct = default)
        => SendAsync<object>(HttpMethod.Post, $"jobs/{jobId}/progress", request, authenticated: true, ct);

    public Task<CoordinatorResult<JobUploadUrlResponse>> UploadUrlsAsync(long jobId, JobUploadUrlRequest request, CancellationToken ct = default)
        => SendAsync<JobUploadUrlResponse>(HttpMethod.Post, $"jobs/{jobId}/uploads", request, authenticated: true, ct);

    public Task<CoordinatorResult<object>> CompleteAsync(long jobId, JobCompleteRequest request, CancellationToken ct = default)
        => SendAsync<object>(HttpMethod.Post, $"jobs/{jobId}/complete", request, authenticated: true, ct);

    public Task<CoordinatorResult<object>> FailAsync(long jobId, JobFailRequest request, CancellationToken ct = default)
        => SendAsync<object>(HttpMethod.Post, $"jobs/{jobId}/fail", request, authenticated: true, ct);

    private async Task<CoordinatorResult<T>> SendAsync<T>(
        HttpMethod method, string path, object body, bool authenticated, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, Url(path))
            {
                Content = JsonContent.Create(body, options: Options)
            };

            if (authenticated)
            {
                request.Headers.Add("X-Node-Token", _token);
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(CallTimeout);

            using var response = await http.SendAsync(request, timeout.Token);
            var payload = await response.Content.ReadAsStringAsync(timeout.Token);

            if (!response.IsSuccessStatusCode)
            {
                var message = TryReadMessage(payload) ?? response.ReasonPhrase ?? "Request failed";
                log.Warn("api", $"{method} {path} → {(int)response.StatusCode}: {message}");

                return new CoordinatorResult<T>(response.StatusCode, default, message);
            }

            var envelope = JsonSerializer.Deserialize<Envelope<T>>(payload, Options);

            return new CoordinatorResult<T>(response.StatusCode, envelope is null ? default : envelope.Data, envelope?.Message);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            log.Warn("api", $"{method} {path} alınmadı: {ex.Message}");

            return new CoordinatorResult<T>(HttpStatusCode.ServiceUnavailable, default, ex.Message);
        }
    }

    private static string? TryReadMessage(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);

            return document.RootElement.TryGetProperty("message", out var message) ? message.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
