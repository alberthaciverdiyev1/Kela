using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Permission.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
