namespace Kela.Web.Models.Workspaces;

public sealed record WorkspaceShellViewModel(
    int Id,
    string Name,
    int StudentCount,
    DateTime CreatedAt);
