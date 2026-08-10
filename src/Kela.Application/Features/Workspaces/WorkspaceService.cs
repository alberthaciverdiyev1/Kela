using FluentValidation;
using Kela.Application.Features.Users;
using Kela.Application.Features.Workspaces.Requests;
using Kela.Application.Features.Workspaces.Responses;
using Kela.Application.Pagination;
using Kela.Application.Patterns;
using Kela.Domain.Common;
using Kela.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Workspaces;

internal sealed class WorkspaceService(
    IWorkspaceRepository workspaces,
    IUserRepository users,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    IValidator<CreateWorkspaceRequest> createValidator,
    IValidator<UpdateWorkspaceRequest> updateValidator) : IWorkspaceService
{
    public async Task<PaginatedResult<WorkspaceResponse>> GetPageAsync(
        int teacherId, int page, CancellationToken cancellationToken = default)
    {
        var result = await workspaces.GetPageAsync(teacherId, page, cancellationToken);
        return new PaginatedResult<WorkspaceResponse>(
            result.Items.Select(w => w.ToResponse()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<WorkspaceDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var workspace = await workspaces.GetByIdAsync(id, cancellationToken);
        return workspace?.ToDetailResponse();
    }

    public async Task<int> CreateAsync(
        CreateWorkspaceRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var trimmed = request.Name.Trim();

        await EnsureTeacherExistsAsync(request.TeacherId, cancellationToken);

        if (await workspaces.NameExistsAsync(request.TeacherId, trimmed, cancellationToken))
        {
            throw new InvalidOperationException($"'{trimmed}' adlı iş alanı zaten kayıtlı.");
        }

        var workspace = new Workspace
        {
            Name = trimmed,
            TeacherId = request.TeacherId,
            CreatedAt = DateTime.UtcNow,
        };

        workspaces.Add(workspace);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return workspace.Id;
    }

    public async Task UpdateAsync(
        int id, UpdateWorkspaceRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var workspace = await workspaces.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan iş alanı bulunamadı.");

        var trimmed = request.Name.Trim();
        if (trimmed != workspace.Name && await workspaces.NameExistsAsync(workspace.TeacherId ?? 0, trimmed, cancellationToken))
        {
            throw new InvalidOperationException($"'{trimmed}' adlı iş alanı zaten kayıtlı.");
        }

        workspace.Name = trimmed;
        workspace.UpdatedAt = DateTime.UtcNow;

        workspaces.Update(workspace);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var workspace = await workspaces.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan iş alanı bulunamadı.");

        workspaces.Remove(workspace);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStudentsAsync(
        int id, AddStudentsRequest request, CancellationToken cancellationToken = default)
    {
        var workspace = await workspaces.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan iş alanı bulunamadı.");

        foreach (var studentId in request.StudentIds.Distinct())
        {
            var user = await userManager.FindByIdAsync(studentId.ToString());
            if (user is null || !await userManager.IsInRoleAsync(user, RoleNames.Student))
            {
                throw new InvalidOperationException($"Id = {studentId} olan öğrenci bulunamadı.");
            }

            if (!workspace.Students.Any(s => s.Id == studentId))
            {
                workspace.Students.Add(user);
            }
        }

        workspace.UpdatedAt = DateTime.UtcNow;
        workspaces.Update(workspace);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveStudentAsync(
        int id, int studentId, CancellationToken cancellationToken = default)
    {
        var workspace = await workspaces.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan iş alanı bulunamadı.");

        var student = workspace.Students.FirstOrDefault(s => s.Id == studentId);
        if (student is not null)
        {
            workspace.Students.Remove(student);
            workspace.UpdatedAt = DateTime.UtcNow;
            workspaces.Update(workspace);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureTeacherExistsAsync(int teacherId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(teacherId, cancellationToken);
        if (user is null || !await userManager.IsInRoleAsync(user, RoleNames.Teacher))
        {
            throw new InvalidOperationException($"Id = {teacherId} olan öğretmen bulunamadı.");
        }
    }
}
