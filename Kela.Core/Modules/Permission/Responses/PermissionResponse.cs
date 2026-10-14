namespace Kela.Core.Modules.Permission.Responses;

public record PermissionResponse(string Name, IReadOnlyList<string> Roles);
