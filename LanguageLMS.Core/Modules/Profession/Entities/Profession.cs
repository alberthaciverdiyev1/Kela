using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Profession.Entities;

public class Profession : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Image { get; set; }

    public long? ParentId { get; set; }

    public long? UserId { get; set; }

    public Profession? Parent { get; set; }

    public ICollection<Profession> Children { get; set; } = new List<Profession>();
}
