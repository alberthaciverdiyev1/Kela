using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Subscription.Entities;

/// <summary>
/// Planı dəyişmədən limit artırmaq üçün müəllim/kurs müraciəti. Admin təsdiqləyəndə
/// müraciətdən <see cref="TenantAddon"/> yaranır.
/// </summary>
public class AddonRequest : BaseEntity
{
    public string Kind { get; set; } = string.Empty;

    /// <summary>Tələbə/qrup sayı, video üçün saat.</summary>
    public int Quantity { get; set; }

    /// <summary>one_time | monthly — müraciətdə göstərilmir, superadmin təsdiqdə seçir.</summary>
    public string Mode { get; set; } = AddonModes.OneTime;

    /// <summary>now | with_plan — müraciət edənin seçdiyi ödəniş vaxtı.</summary>
    public string PaymentTiming { get; set; } = AddonPaymentTimings.Now;

    /// <summary>pending | approved | rejected</summary>
    public string Status { get; set; } = AddonRequestStatuses.Pending;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "AZN";

    /// <summary>Müraciət edənin qeydi.</summary>
    public string? Note { get; set; }

    /// <summary>Adminin cavab qeydi.</summary>
    public string? ReviewNote { get; set; }

    public long? RequestedByUserId { get; set; }

    public string? RequestedByName { get; set; }

    public long? ReviewedByUserId { get; set; }

    public string? ReviewedByName { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    /// <summary>Təsdiqdən sonra yaranan əlavə paket.</summary>
    public long? AddonId { get; set; }
}

public static class AddonModes
{
    /// <summary>Bir dəfəlik — yalnız təsdiqləndiyi ay üçün keçərli.</summary>
    public const string OneTime = "one_time";

    /// <summary>Aylıq — ləğv edilənə qədər hər ay tətbiq olunur.</summary>
    public const string Monthly = "monthly";

    public static readonly string[] All = [OneTime, Monthly];
}

public static class AddonRequestStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";

    public static readonly string[] All = [Pending, Approved, Rejected];
}
