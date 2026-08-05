using Kela.Domain.Common;
using Kela.Domain.Subjects;

namespace Kela.Domain.Users;

public class Teacher : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
