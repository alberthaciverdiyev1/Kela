using Kela.Domain.Common;

namespace Kela.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? TeacherId { get; set; }
    public User? Teacher { get; set; }

    public ICollection<User> Students { get; set; } = new List<User>();
}
