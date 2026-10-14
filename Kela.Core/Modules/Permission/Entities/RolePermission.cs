using Kela.Core.Common.Entities;

namespace Kela.Core.Modules.Permission.Entities;

public class RolePermission : BaseEntity
{
    public string Role { get; set; } = string.Empty;

    public long PermissionId { get; set; }

    public Permission? Permission { get; set; }
}
