using Kela.Domain.Common;
using Kela.Domain.Enums;

namespace Kela.Domain.Entities;

public class StudentPaymentTrack : BaseEntity
{
    public int StudentId { get; set; }
    public User? Student { get; set; }

    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public StudentPaymentTrackStatus Status { get; set; }
}
