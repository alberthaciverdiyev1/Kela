namespace Kela.Domain.Common;

public static class LanguageCodes
{
    public const string Az = "az";
    public const string En = "en";
    public const string Ru = "ru";
    public const string Tr = "tr";

    public static readonly IReadOnlyList<string> All = new[] { Az, En, Ru, Tr };

    public static bool IsSupported(string? language)
        => !string.IsNullOrWhiteSpace(language) && All.Contains(language, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string? language)
        => IsSupported(language) ? language!.Trim().ToLowerInvariant() : Tr;
}
