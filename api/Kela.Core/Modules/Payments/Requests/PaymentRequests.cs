namespace Kela.Core.Modules.Payments.Requests;

public sealed class SavePaymentPlanRequest
{
    public long StudentId { get; set; }
    public DateOnly JoinedOn { get; set; }
    public DateOnly NextDueOn { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Frequency { get; set; } = "monthly";
    public int? IntervalDays { get; set; }
    public string? Note { get; set; }
}

public sealed class RecordPaymentRequest
{
    public long StudentId { get; set; }
    public DateOnly PaidOn { get; set; }
    public decimal? Amount { get; set; }
    public string? Note { get; set; }
}

public sealed class CreateDefaultPlanRequest
{
    public DateOnly JoinedOn { get; set; }
    public DateOnly NextDueOn { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Frequency { get; set; } = "monthly";
    public int? IntervalDays { get; set; }
    public string? Note { get; set; }
}
