namespace Kela.Application.Features.Nodes.Requests;

public sealed record AddContentRequest(
    int WorkspaceId,
    int ContentId,
    int? ParentId);
