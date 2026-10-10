using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Group.Entities;

using User = LanguageLMS.Core.Modules.User.Entities.User;

public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Image { get; set; }

    public long? UserId { get; set; }

    public User? Teacher { get; set; }

    public ICollection<StudentGroup> Memberships { get; set; } = new List<StudentGroup>();

    public ICollection<GroupSubject> SubjectLinks { get; set; } = new List<GroupSubject>();
}
