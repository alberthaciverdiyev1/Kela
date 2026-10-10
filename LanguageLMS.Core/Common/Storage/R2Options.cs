namespace LanguageLMS.Core.Common.Storage;

/// <summary>Cloudflare R2 (S3-uyğun) konfiqurasiyası.</summary>
public class R2Options
{
    public const string SectionName = "R2";

    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;

    /// <summary>Public baza URL (R2 dev link və ya custom domain). Məs: https://pub-xxxx.r2.dev</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;
}
