using Kela.Domain.Common;
using Kela.Domain.Users;
using Kela.Domain.Users.Enums;

namespace Kela.Domain;

public class PaymentTrack : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public PaymentTrackStatus  Status { get; set; }

}
