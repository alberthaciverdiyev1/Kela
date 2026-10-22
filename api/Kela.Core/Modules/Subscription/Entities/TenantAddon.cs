using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Subscription.Entities;

/// <summary>Alınmış əlavə paketin tarixçəsi — müştəriyə hesabat üçün.</summary>
public class TenantAddon : BaseEntity
{
    public string Period { get; set; } = string.Empty;

    /// <summary>students | video | storage</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Paket sayı (neçə paket alınıb).</summary>
    public int Packs { get; set; } = 1;

    /// <summary>Limitə əlavə olunan miqdar (tələbə sayı / dəqiqə / bayt).</summary>
    public int Quantity { get; set; }

    public long Bytes { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "AZN";

    public string? Note { get; set; }
}

public static class AddonKinds
{
    public const string Students = "students";
    public const string Video = "video";
    public const string Storage = "storage";

    public static readonly string[] All = [Students, Video, Storage];
}
