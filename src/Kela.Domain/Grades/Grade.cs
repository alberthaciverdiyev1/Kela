using Kela.Domain.Common;
using Kela.Domain.Users;

namespace Kela.Domain.Grades;

public class Grade : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
