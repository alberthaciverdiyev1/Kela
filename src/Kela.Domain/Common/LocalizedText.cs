namespace Kela.Domain.Common;

public static class LocalizedText
{
    public static string Get(IReadOnlyDictionary<string, string> translations, string? language)
    {
        var lang = LanguageCodes.Normalize(language);

        if (translations.TryGetValue(lang, out var direct))
            return direct;
        if (translations.TryGetValue(LanguageCodes.En, out var en))
            return en;
        if (translations.TryGetValue(LanguageCodes.Az, out var az))
            return az;

        return translations.Values.FirstOrDefault() ?? string.Empty;
    }
}
