namespace LanguageLMS.Core.Common.Storage;

public class BunnyCdnOptions
{
    public const string SectionName = "BunnyCdn";

    public string StorageZone { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Pull zone hostname (örn. myzone.b-cdn.net) veya tam URL.</summary>
    public string PullZone { get; set; } = string.Empty;

    /// <summary>Bölge kodu (örn. "ny", "de"). Boşsa default (storage.bunnycdn.com).</summary>
    public string Region { get; set; } = string.Empty;
}
