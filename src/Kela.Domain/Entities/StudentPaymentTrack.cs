using Kela.Domain.Common;
using Kela.Domain.Enums;

namespace Kela.Domain.Entities;

/// <summary>
/// Öğrencinin ödeme takibi (ücret/taksit). Ödemeler Student rolündeki kullanıcıya aittir.
/// </summary>
public class StudentPaymentTrack : BaseEntity
{
    /// <summary>FK → AspNetUsers (Student rolündeki kullanıcı).</summary>
    public int StudentId { get; set; }
    public User? Student { get; set; }

    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public StudentPaymentTrackStatus Status { get; set; }
}
