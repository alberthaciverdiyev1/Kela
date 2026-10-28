using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Subscription.Entities;

/// <summary>Alınmış əlavə paketin tarixçəsi — müştəriyə hesabat üçün.</summary>
public class TenantAddon : BaseEntity
{
    public string Period { get; set; } = string.Empty;

    /// <summary>students | groups | video</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Limitə əlavə olunan miqdar (tələbə / qrup sayı, video üçün saat).</summary>
    public int Quantity { get; set; }

    /// <summary>one_time | monthly — aylıq paket ləğv edilənə qədər hər ay tətbiq olunur.</summary>
    public string Mode { get; set; } = AddonModes.OneTime;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "AZN";

    public string? Note { get; set; }
}

public static class AddonKinds
{
    public const string Students = "students";
    public const string Groups = "groups";
    public const string Video = "video";

    public static readonly string[] All = [Students, Groups, Video];
}
