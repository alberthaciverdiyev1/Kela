using Kela.Domain.Common;

namespace Kela.Domain.Users;

public class Teacher : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
