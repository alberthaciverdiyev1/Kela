namespace Kela.App.Shared.Dtos;

public record ContentItemDto(long Id, string Title, string? Description, string? Type, string? Url,
    string? PlaybackUrl, int? DurationSeconds)
{
    public string TypeLabel => Type switch
    {
        "video" => "Video",
        "file" => "Fayl",
        "voice" => "Səs yazısı",
        _ => "Material"
    };

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
