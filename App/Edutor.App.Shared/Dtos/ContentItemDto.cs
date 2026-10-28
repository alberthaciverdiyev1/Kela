namespace Edutor.App.Shared.Dtos;

public record ContentItemDto(long Id, string Title, string? Description, string? Type, string? Url,
    string? PlaybackUrl, int? DurationSeconds, string? ThumbnailUrl = null, string? ConversionStatus = null)
{
    public string TypeLabel => Type switch
    {
        "video" => "Video",
        "file" => "Fayl",
        "voice" => "Səs yazısı",
        _ => "Material"
    };

    /// <summary>Video konversiyası hələ bitməyibsə oynatmaq mənasızdır.</summary>
    public bool IsProcessing => Type == "video"
        && ConversionStatus is "pending" or "processing";

    /// <summary>Konversiya alınmadı.</summary>
    public bool IsFailed => Type == "video" && ConversionStatus == "failed";

    /// <summary>Müddət: saat:dəqiqə:saniyə (məs. 1:06:27) və ya dəqiqə:saniyə (6:27).</summary>
    public string DurationLabel
    {
        get
        {
            if (DurationSeconds is not > 0) return "";
            var t = TimeSpan.FromSeconds(DurationSeconds.Value);
            return t.TotalHours >= 1
                ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}"
                : $"{t.Minutes}:{t.Seconds:00}";
        }
    }
}
