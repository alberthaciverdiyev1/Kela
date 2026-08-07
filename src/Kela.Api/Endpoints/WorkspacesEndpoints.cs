using Kela.Api.Contracts;
using Kela.Application.Features.Workspaces;
using Kela.Application.Features.Workspaces.Requests;
using Kela.Application.Features.Workspaces.Responses;
using Kela.Application.Pagination;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

public static class WorkspacesEndpoints
{
    public static IEndpointRouteBuilder MapWorkspacesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workspaces")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Teacher, RoleNames.Admin));

        group.MapGet("", async (int teacherId, int page, int pageSize, IWorkspaceService workspaces, CancellationToken ct) =>
            ApiResponse<PaginatedResult<WorkspaceResponse>>.Success(
                await workspaces.GetPageAsync(teacherId, page, pageSize, ct)));

        group.MapGet("/{id:int}", async (int id, IWorkspaceService workspaces, CancellationToken ct) =>
        {
            var workspace = await workspaces.GetByIdAsync(id, ct);
            return workspace is null
                ? ApiResponse.NotFound("İş alanı bulunamadı.")
                : ApiResponse<WorkspaceDetailResponse>.Success(workspace);
        });

        group.MapPost("", async (CreateWorkspaceRequest request, IWorkspaceService workspaces, CancellationToken ct) =>
        {
            var id = await workspaces.CreateAsync(request, ct);
            return ApiResponse<object>.Created($"/api/workspaces/{id}", new { id });
        });

        group.MapPut("/{id:int}", async (int id, UpdateWorkspaceRequest request, IWorkspaceService workspaces, CancellationToken ct) =>
        {
            await workspaces.UpdateAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, IWorkspaceService workspaces, CancellationToken ct) =>
        {
            await workspaces.DeleteAsync(id, ct);
            return ApiResponse.NoContent();
        });

        group.MapPost("/{id:int}/students", async (int id, AddStudentsRequest request, IWorkspaceService workspaces, CancellationToken ct) =>
        {
            await workspaces.AddStudentsAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}/students/{studentId:int}", async (int id, int studentId, IWorkspaceService workspaces, CancellationToken ct) =>
        {
            await workspaces.RemoveStudentAsync(id, studentId, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
