using Kela.Application.Features.Workspaces.Responses;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Workspaces;

public static class WorkspaceMappings
{
    public static WorkspaceResponse ToResponse(this Workspace workspace) => new(
        workspace.Id,
        workspace.Name,
        workspace.TeacherId,
        workspace.Teacher is null ? null : $"{workspace.Teacher.FirstName} {workspace.Teacher.LastName}".Trim(),
        workspace.Students.Count,
        workspace.CreatedAt);

    public static WorkspaceStudentResponse ToStudentResponse(this User student) => new(
        student.Id,
        student.FirstName,
        student.LastName ?? string.Empty,
        student.Email ?? string.Empty);

    public static WorkspaceDetailResponse ToDetailResponse(this Workspace workspace) => new(
        workspace.Id,
        workspace.Name,
        workspace.TeacherId,
        workspace.Teacher is null ? null : $"{workspace.Teacher.FirstName} {workspace.Teacher.LastName}".Trim(),
        workspace.Students.Count,
        workspace.CreatedAt,
        workspace.Students.OrderBy(s => s.FirstName).Select(s => s.ToStudentResponse()).ToList());
}
