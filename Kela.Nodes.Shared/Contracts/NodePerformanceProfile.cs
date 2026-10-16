namespace Kela.Nodes.Shared.Contracts;

/// <summary>
/// Cihazın resurs profili. <see cref="Auto"/> seçildikdə worker cihazın ölçülmüş
/// gücünə (nüvə sayı, aparat encoder-i, batareya) uyğun profili özü seçir.
/// </summary>
public static class NodePerformanceProfile
{
    /// <summary>Cihaza uyğun avtomatik seçim.</summary>
    public const string Auto = "auto";

    /// <summary>Bütün nüvələr, ən sürətli preset, maksimum paralel iş.</summary>
    public const string Max = "max";

    /// <summary>Bir nüvə tətbiqə saxlanılır, ölçülü paralellik — default.</summary>
    public const string Balanced = "balanced";

    /// <summary>Yarı nüvə, aşağı proses prioriteti, tək iş — cihaz istifadəçiyə qalsın.</summary>
    public const string Low = "low";

    public static readonly string[] All = [Auto, Max, Balanced, Low];

    public static bool IsValid(string? value) => value is not null && Array.IndexOf(All, value) >= 0;

    public static string Normalize(string? value) => IsValid(value) ? value! : Auto;

    public static string Describe(string profile) => profile switch
    {
        Max => "Maksimum güc",
        Low => "Aşağı resurs",
        _ => "Balanslı"
    };
}
