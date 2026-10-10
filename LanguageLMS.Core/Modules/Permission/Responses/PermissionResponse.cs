namespace LanguageLMS.Core.Modules.Permission.Responses;

public record PermissionResponse(string Name, IReadOnlyList<string> Roles);
