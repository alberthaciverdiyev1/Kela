using Kela.Domain.Common;
using Kela.Domain.Grades;
using Kela.Domain.Subjects;

namespace Kela.Domain.Users;

public class Student : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
