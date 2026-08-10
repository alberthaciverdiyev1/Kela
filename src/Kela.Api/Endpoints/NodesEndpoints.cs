using Kela.Api.Contracts;
using Kela.Application.Features.Nodes;
using Kela.Application.Features.Nodes.Requests;
using Kela.Application.Features.Nodes.Responses;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

public static class NodesEndpoints
{
    public static IEndpointRouteBuilder MapNodesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/nodes")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Teacher, RoleNames.Admin));

        group.MapGet("/tree", async (int? workspaceId, int? teacherId, INodeService nodes, CancellationToken ct) =>
        {
            if (workspaceId is not null)
            {
                return ApiResponse<List<NodeResponse>>.Success(await nodes.GetWorkspaceTreeAsync(workspaceId.Value, ct));
            }

            if (teacherId is not null)
            {
                return ApiResponse<List<NodeResponse>>.Success(await nodes.GetLibraryTreeAsync(teacherId.Value, ct));
            }

            return ApiResponse.BadRequest("workspaceId veya teacherId belirtilmelidir.");
        });

        group.MapPost("/folder", async (CreateFolderRequest request, INodeService nodes, CancellationToken ct) =>
        {
            var id = await nodes.CreateFolderAsync(request, ct);
            return ApiResponse<int>.Created($"/api/nodes/{id}", id);
        });

        group.MapPost("/content", async (AddContentRequest request, INodeService nodes, CancellationToken ct) =>
        {
            var id = await nodes.AddContentAsync(request, ct);
            return ApiResponse<int>.Created($"/api/nodes/{id}", id);
        });

        group.MapPost("/copy-folder", async (CopyFolderRequest request, INodeService nodes, CancellationToken ct) =>
        {
            var id = await nodes.CopyFolderAsync(request, ct);
            return ApiResponse<int>.Created($"/api/nodes/{id}", id);
        });

        group.MapPut("/{id:int}", async (int id, UpdateNodeRequest request, INodeService nodes, CancellationToken ct) =>
        {
            await nodes.UpdateNodeAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, INodeService nodes, CancellationToken ct) =>
        {
            await nodes.DeleteNodeAsync(id, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
