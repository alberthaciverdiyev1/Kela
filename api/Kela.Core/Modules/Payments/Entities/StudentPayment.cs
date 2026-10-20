using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Payments.Entities;

public class StudentPayment : BaseEntity
{
    public long StudentId { get; set; }
    public DateOnly PaidOn { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AZN";
    public string? Note { get; set; }
}
