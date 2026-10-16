using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Media.Entities;

public class Video : BaseEntity
{
    public string? Title { get; set; }

    public string Path { get; set; } = string.Empty;

    public string Type { get; set; } = "video";

    public int? DurationSeconds { get; set; }

    /// <summary>HLS (m3u8) playback URL — konvertordan sonra doldurulur.</summary>
    public string? PlaybackUrl { get; set; }

    /// <summary>Kadr önizləməsi (thumbnail) yolu — xarici worker tərəfindən yaradılır.</summary>
    public string? Thumbnail { get; set; }

    /// <summary>Konversiya vəziyyəti — bax: <see cref="VideoStatus"/>.</summary>
    public string ConversionStatus { get; set; } = VideoStatus.Pending;

    /// <summary>Konversiya xətası (yalnız failed olduqda).</summary>
    public string? ConversionError { get; set; }

    /// <summary>
    /// Neçə dəfə tam konversiya cəhdi edilib. Keçici xəta bütün tapşırıqları
    /// uğursuz etsə, video dərhal "failed" olmur — bir neçə dəfə yenidən sınanır.
    /// </summary>
    public int ConversionAttempts { get; set; }

    /// <summary>
    /// Konversiya versiyası. Yenidən konversiya başladıqda artırılır ki, yeni çıxışlar
    /// ayrı qovluğa düşsün və hazır olana qədər köhnə versiya yayımda qalsın.
    /// </summary>
    public int ConversionVersion { get; set; } = 1;
}
