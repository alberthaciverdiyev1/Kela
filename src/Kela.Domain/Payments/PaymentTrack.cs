using Kela.Domain.Common;
using Kela.Domain.Payments.Enums;
using Kela.Domain.Users;

namespace Kela.Domain.Payments;

public class PaymentTrack : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public PaymentTrackStatus Status { get; set; }

}
