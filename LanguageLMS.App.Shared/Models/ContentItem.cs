namespace LanguageLMS.App.Shared.Models;

public record ContentItem(long Id, string Title, string? Description, string? Type, string? Url,
    string? PlaybackUrl, int? DurationSeconds)
{
    public string TypeLabel => Type switch
    {
        "video" => "Video",
        "file" => "Fayl",
        "voice" => "Səs yazısı",
        _ => "Material"
    };
}
