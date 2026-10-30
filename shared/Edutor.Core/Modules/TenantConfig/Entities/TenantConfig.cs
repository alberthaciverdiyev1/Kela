using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.TenantConfig.Entities;

public class TenantConfig : BaseEntity
{
    /// <summary>none · all · groups — ödənişi gecikən şagirdin girişinə hansı səviyyədə məhdudiyyət qoyulur.</summary>
    public string PaymentBlockMode { get; set; } = PaymentBlockModes.None;

    /// <summary>Vergüllə ayrılmış qrup id-ləri — yalnız <see cref="PaymentBlockModes.Groups"/> rejimində istifadə olunur.</summary>
    public string? PaymentBlockedGroupIds { get; set; }
}

public static class PaymentBlockModes
{
    public const string None = "none";
    public const string All = "all";
    public const string Groups = "groups";

    public static bool IsValid(string? value) => value is None or All or Groups;
}
