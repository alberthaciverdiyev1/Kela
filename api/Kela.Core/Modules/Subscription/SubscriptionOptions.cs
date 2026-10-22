namespace Kela.Core.Modules.Subscription;

public class SubscriptionOptions
{
    public const string SectionName = "Subscription";

    /// <summary>
    /// Tam bucket taramasının dövrü (saat). Gündəlik istifadə hər yükləmədə artımlı
    /// sayılır; bu tarama yalnız sapmanı düzəldir və köhnə orijinalları silir.
    /// 0 = söndürülüb.
    /// </summary>
    public int MeteringHours { get; set; } = 24;

    /// <summary>Fon ölçmə dövrü işləsin (testlərdə söndürülür).</summary>
    public bool BackgroundEnabled { get; set; } = true;

    /// <summary>
    /// Planın "orijinal saxlama" müddəti bitmiş video fayllarını sil.
    /// Söndürülübsə heç nə silinmir — yalnız ölçmə aparılır.
    /// </summary>
    public bool DeleteExpiredOriginals { get; set; } = false;
}
