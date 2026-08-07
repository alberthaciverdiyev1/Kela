namespace Kela.Application.Features.Workspaces.Responses;

public sealed record WorkspaceResponse(
    int Id,
    string Name,
    int? TeacherId,
    string? TeacherName,
    int StudentCount,
    DateTime CreatedAt);
