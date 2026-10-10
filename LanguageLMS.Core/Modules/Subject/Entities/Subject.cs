using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Subject.Entities;

public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public long? UserId { get; set; }
}
