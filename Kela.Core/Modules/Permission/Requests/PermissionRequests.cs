namespace Kela.Core.Modules.Permission.Requests;

public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
}

public class SetRolePermissionsRequest
{
    public string Role { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = new();
}
