using Microsoft.Extensions.Logging;

namespace Kela.Core.Common.Push;

/// <summary>FCM açarları verilməyincə istifadə olunur: sadəcə log yazır.</summary>
public sealed class NoOpPushSender(ILogger<NoOpPushSender> logger) : IPushSender
{
    public Task SendAsync(
        IReadOnlyCollection<string> tokens,
        string title,
        string? body,
        string? image = null,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        logger.LogInformation("Push skipped (no provider). Tokens={Count} Title={Title}", tokens.Count, title);
        return Task.CompletedTask;
    }
}
