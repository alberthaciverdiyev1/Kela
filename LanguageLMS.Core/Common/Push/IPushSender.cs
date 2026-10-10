namespace LanguageLMS.Core.Common.Push;

public interface IPushSender
{
    Task SendAsync(
        IReadOnlyCollection<string> tokens,
        string title,
        string? body,
        string? image = null,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken ct = default);
}
