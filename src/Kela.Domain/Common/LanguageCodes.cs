namespace Kela.Domain.Common;

/// <summary>
/// Uygulama geneli desteklenen dil kodları (az/en/ru/tr).
/// Çevirisi olan HER entity bu tek kaynağı kullanır — yeni dil eklemek
/// için tek yer burasıdır.
/// </summary>
public static class LanguageCodes
{
    public const string Az = "az";
    public const string En = "en";
    public const string Ru = "ru";
    public const string Tr = "tr";

    public static readonly IReadOnlyList<string> All = new[] { Az, En, Ru, Tr };

    public static bool IsSupported(string? language)
        => !string.IsNullOrWhiteSpace(language) && All.Contains(language, StringComparer.OrdinalIgnoreCase);

    /// <summary>Geçerli dili normalize eder; geçersiz/eksikse varsayılan "tr" döner.</summary>
    public static string Normalize(string? language)
        => IsSupported(language) ? language!.Trim().ToLowerInvariant() : Tr;
}
