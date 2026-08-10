namespace Kela.Application.Features.Nodes.Requests;

public sealed record CopyFolderRequest(
    int WorkspaceId,
    int SourceNodeId,
    int? ParentId);
