using Kela.Domain.Common;
using Kela.Domain.Payments.Enums;
using Kela.Domain.Users;

namespace Kela.Domain.Payments;

/// <summary>
/// Öğrencinin ödeme takibi (ücret/taksit). Ödemeler öğrenciye aittir —
/// User'a değil Student'a bağlanır.
/// </summary>
public class PaymentTrack : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }

    /// <summary>FK → students.UserId (Student'in primary key'i).</summary>
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public PaymentTrackStatus Status { get; set; }
}
