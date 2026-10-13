namespace LanguageLMS.Core.Modules.Certificate;

/// <summary>Mövcud sertifikat şablonları (UI-da seçim üçün).</summary>
public static class CertificateTemplates
{
    public static readonly IReadOnlyList<CertificateTemplate> All =
    [
        new("classic", "Klassik"),
        new("modern", "Müasir"),
        new("elegant", "Eleqant"),
        new("minimal", "Minimal")
    ];

    public static string NameOf(string key)
        => All.FirstOrDefault(t => t.Key == key)?.Name ?? key;

    public static bool Exists(string key)
        => All.Any(t => t.Key == key);
}

public record CertificateTemplate(string Key, string Name);
