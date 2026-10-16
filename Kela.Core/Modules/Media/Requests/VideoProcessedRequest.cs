using Kela.Core.Modules.Media.Entities;

namespace Kela.Core.Modules.Media.Requests;

/// <summary>
/// Xarici worker-ın konversiya nəticəsi. R2 event-i yalnız obyekt açarını verdiyi üçün
/// video <see cref="SourcePath"/> ilə tapılır, ID ilə yox.
/// </summary>
public class VideoProcessedRequest
{
    /// <summary>Yüklənən orijinal faylın storage açarı (Video.Path ilə eyni olmalıdır).</summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>m3u8 faylının yolu və ya tam URL-i.</summary>
    public string? PlaybackUrl { get; set; }

    /// <summary>Thumbnail yolu və ya tam URL-i.</summary>
    public string? Thumbnail { get; set; }

    public int? DurationSeconds { get; set; }

    /// <summary>ready | failed</summary>
    public string Status { get; set; } = VideoStatus.Ready;

    /// <summary>Xəta mətni (yalnız failed olduqda).</summary>
    public string? Error { get; set; }
}
