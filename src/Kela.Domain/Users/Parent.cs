using Kela.Domain.Common;

namespace Kela.Domain.Users;

public class Parent : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
