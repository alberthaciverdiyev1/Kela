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

    /// <summary>now | with_plan — indi ödənilib və ya ay sonunda planla birlikdə.</summary>
    public string PaymentTiming { get; set; } = AddonPaymentTimings.Now;

    /// <summary>Ödəniş qeydə alınan an. Boşdursa ödəniş hələ gözlənilir.</summary>
    public DateTimeOffset? PaidAt { get; set; }

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

public static class AddonPaymentTimings
{
    /// <summary>Paket alınarkən dərhal ödənilir.</summary>
    public const string Now = "now";

    /// <summary>Ay sonunda planla birlikdə hesablanır.</summary>
    public const string WithPlan = "with_plan";

    public static readonly string[] All = [Now, WithPlan];
}
