using Kela.Api.Contracts;
using Kela.Application.Features.Contents;
using Kela.Application.Features.Contents.Requests;
using Kela.Application.Features.Contents.Responses;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

public static class ContentsEndpoints
{
    public static IEndpointRouteBuilder MapContentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contents")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Teacher, RoleNames.Admin));

        group.MapGet("", async (int teacherId, IContentService contents, CancellationToken ct) =>
            ApiResponse<List<ContentResponse>>.Success(
                await contents.GetByTeacherAsync(teacherId, ct)));

        group.MapGet("/{id:int}", async (int id, IContentService contents, CancellationToken ct) =>
        {
            var content = await contents.GetByIdAsync(id, ct);
            return content is null
                ? ApiResponse.NotFound("İçerik bulunamadı.")
                : ApiResponse<ContentResponse>.Success(content);
        });

        group.MapPost("", async (CreateContentRequest request, IContentService contents, CancellationToken ct) =>
        {
            var id = await contents.CreateAsync(request, ct);
            return ApiResponse<int>.Created($"/api/contents/{id}", id);
        });

        group.MapPut("/{id:int}", async (int id, UpdateContentRequest request, IContentService contents, CancellationToken ct) =>
        {
            await contents.UpdateAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapPut("/{id:int}/publish", async (int id, bool published, IContentService contents, CancellationToken ct) =>
        {
            await contents.SetPublishedAsync(id, published, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, IContentService contents, CancellationToken ct) =>
        {
            await contents.DeleteAsync(id, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
