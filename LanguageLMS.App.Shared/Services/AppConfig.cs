namespace LanguageLMS.App.Shared.Services;

/// <summary>Tətbiq konfiqurasiyası (API ünvanı).</summary>
public static class AppConfig
{
    public const string ApiBaseUrl = "http://localhost:5080";

    /// <summary>Watermark yalnız web-də göstərilir. Native app-də FLAG_SECURE olduğu üçün lazımsızdır.</summary>
    public static bool ShowWatermark { get; set; } = true;
}
