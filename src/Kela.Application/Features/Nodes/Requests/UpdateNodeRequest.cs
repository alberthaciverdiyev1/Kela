namespace Kela.Application.Features.Nodes.Requests;

public sealed record UpdateNodeRequest(
    string? Name,
    int? ParentId,
    int? Position);
