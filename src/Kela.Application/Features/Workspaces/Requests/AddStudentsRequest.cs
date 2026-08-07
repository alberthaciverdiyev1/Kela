namespace Kela.Application.Features.Workspaces.Requests;

public sealed record AddStudentsRequest(IReadOnlyList<int> StudentIds);
