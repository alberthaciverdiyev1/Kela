using System.Text.Json;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Edutor.Core.Common.Push;

// Brauzer Web Push (VAPID) göndəricisi. Token sahəsində subscription JSON-u saxlanır:
// {"endpoint":"...","keys":{"p256dh":"...","auth":"..."}}.
public sealed class WebPushSender : IPushSender
{
    private readonly PushServiceClient _client;
    private readonly ILogger<WebPushSender> _logger;

    public WebPushSender(IOptions<WebPushOptions> options, ILogger<WebPushSender> logger)
    {
        var o = options.Value;
        _logger = logger;
        _client = new PushServiceClient
        {
            DefaultAuthentication = new VapidAuthentication(o.PublicKey, o.PrivateKey) { Subject = o.Subject }
        };
    }

    public async Task SendAsync(
        IReadOnlyCollection<string> tokens,
        string title,
        string? body,
        string? image = null,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(new { title, body, image, data });
        var message = new PushMessage(payload) { Urgency = PushMessageUrgency.Normal };

        foreach (var token in tokens)
        {
            if (!TryParse(token, out var subscription)) continue;

            try
            {
                await _client.RequestPushMessageDeliveryAsync(subscription, message, ct);
            }
            catch (Exception ex)
            {
                // Ölü abunəlikləri (410) burada silmirik — növbəti qeydiyyatda yenilənir.
                _logger.LogWarning(ex, "Web push delivery failed");
            }
        }
    }

    private static bool TryParse(string token, out PushSubscription subscription)
    {
        subscription = null!;
        if (string.IsNullOrWhiteSpace(token) || token.TrimStart()[0] != '{') return false;

        try
        {
            using var document = JsonDocument.Parse(token);
            var root = document.RootElement;
            var endpoint = root.TryGetProperty("endpoint", out var e) ? e.GetString() : null;
            if (!root.TryGetProperty("keys", out var keys)) return false;

            var p256dh = keys.TryGetProperty("p256dh", out var k1) ? k1.GetString() : null;
            var auth = keys.TryGetProperty("auth", out var k2) ? k2.GetString() : null;

            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(p256dh) || string.IsNullOrWhiteSpace(auth))
            {
                return false;
            }

            subscription = new PushSubscription
            {
                Endpoint = endpoint,
                Keys = new Dictionary<string, string>
                {
                    ["p256dh"] = p256dh,
                    ["auth"] = auth
                }
            };
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
