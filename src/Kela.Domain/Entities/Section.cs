using Kela.Domain.Common;

namespace Kela.Domain.Entities;

public class Section : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int? TeacherId { get; set; }
    public User? Teacher { get; set; }

    public ICollection<User> Students { get; set; } = new List<User>();
}
