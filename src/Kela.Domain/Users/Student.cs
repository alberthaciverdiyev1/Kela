using Kela.Domain.Common;
using Kela.Domain.Grades;

namespace Kela.Domain.Users;

public class Student : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
