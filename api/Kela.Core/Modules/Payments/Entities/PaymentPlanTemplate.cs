using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Payments.Entities;

/// <summary>Hazır ödəniş planı şablonu — tələbəyə plan təyin edərkən seçilir.</summary>
public class PaymentPlanTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = nameof(PaymentCurrency.AZN);
    public string Frequency { get; set; } = "monthly";
    public int? IntervalDays { get; set; }
    public string? Note { get; set; }
}
