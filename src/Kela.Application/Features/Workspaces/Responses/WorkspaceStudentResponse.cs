namespace Kela.Application.Features.Workspaces.Responses;

public sealed record WorkspaceStudentResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email);
