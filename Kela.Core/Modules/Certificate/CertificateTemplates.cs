namespace Kela.Core.Modules.Certificate;

/// <summary>Mövcud sertifikat şablonları. Hazırda tək şablon var (first).</summary>
public static class CertificateTemplates
{
    public static readonly IReadOnlyList<CertificateTemplate> All =
    [
        new("first", "Sertifikat")
    ];

    public static string NameOf(string key)
        => All.FirstOrDefault(t => t.Key == key)?.Name ?? key;

    public static bool Exists(string key)
        => All.Any(t => t.Key == key);
}

public record CertificateTemplate(string Key, string Name);
