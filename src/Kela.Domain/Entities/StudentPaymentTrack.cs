using Kela.Domain.Common;
using Kela.Domain.Enums;

namespace Kela.Domain.Entities;

/// <summary>
/// Öğrencinin ödeme takibi (ücret/taksit). Ödemeler öğrenciye aittir —
/// User'a değil Student'a bağlanır.
/// </summary>
public class StudentPaymentTrack : BaseEntity
{
    /// <summary>FK → students.UserId (Student'in primary key'i).</summary>
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public StudentPaymentTrackStatus Status { get; set; }
}
