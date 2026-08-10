namespace Kela.Application.Features.Nodes.Requests;

public sealed record CreateFolderRequest(
    int? WorkspaceId,
    int? TeacherId,
    string Name,
    int? ParentId);
