namespace Kela.Application.Features.Workspaces.Responses;

public sealed record WorkspaceDetailResponse(
    int Id,
    string Name,
    int? TeacherId,
    string? TeacherName,
    int StudentCount,
    DateTime CreatedAt,
    IReadOnlyList<WorkspaceStudentResponse> Students);
