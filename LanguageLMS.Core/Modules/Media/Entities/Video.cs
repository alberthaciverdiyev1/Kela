using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Media.Entities;

public class Video : BaseEntity
{
    public string? Title { get; set; }

    public string Path { get; set; } = string.Empty;

    public string Type { get; set; } = "video";

    public int? DurationSeconds { get; set; }

    /// <summary>HLS (m3u8) playback URL — konvertordan sonra doldurulur.</summary>
    public string? PlaybackUrl { get; set; }
}
