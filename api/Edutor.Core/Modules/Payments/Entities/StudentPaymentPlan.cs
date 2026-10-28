using Edutor.Core.Common.Entities;

namespace Edutor.Core.Modules.Payments.Entities;

public class StudentPaymentPlan : BaseEntity
{
    public long StudentId { get; set; }
    public DateOnly JoinedOn { get; set; }
    public DateOnly NextDueOn { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Frequency { get; set; } = "monthly";
    public int? IntervalDays { get; set; }
    public DateOnly? LastPaidOn { get; set; }
    public string? Note { get; set; }
}
