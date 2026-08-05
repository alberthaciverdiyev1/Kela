using Kela.Domain.Common;

namespace Kela.Domain.Users;

public class Parent : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
