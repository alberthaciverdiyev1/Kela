namespace Edutor.Core.Common.Push;

public class FcmOptions
{
    public const string SectionName = "Fcm";

    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Service account JSON faylının yolu (məs. secrets/fcm-service-account.json).</summary>
    public string ServiceAccountPath { get; set; } = string.Empty;
}
