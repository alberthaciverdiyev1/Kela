namespace Edutor.Core.Common.Push;

public class WebPushOptions
{
    public const string SectionName = "WebPush";

    /// <summary>VAPID əlaqəsi — "mailto:" və ya sayt URL-i.</summary>
    public string Subject { get; set; } = string.Empty;

    public string PublicKey { get; set; } = string.Empty;

    public string PrivateKey { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Subject)
        && !string.IsNullOrWhiteSpace(PublicKey)
        && !string.IsNullOrWhiteSpace(PrivateKey);
}
