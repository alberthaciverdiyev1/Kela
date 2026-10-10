using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.Permission.Entities;

public class RolePermission : BaseEntity
{
    public string Role { get; set; } = string.Empty;

    public long PermissionId { get; set; }

    public Permission? Permission { get; set; }
}
