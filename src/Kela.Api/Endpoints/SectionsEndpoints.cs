using Kela.Api.Contracts;
using Kela.Application.Features.Sections;
using Kela.Application.Features.Sections.Requests;
using Kela.Application.Features.Sections.Responses;
using Kela.Application.Pagination;

namespace Kela.Api.Endpoints;

public static class SectionsEndpoints
{
    public static IEndpointRouteBuilder MapSectionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sections");

        group.MapGet("", async (int page, int pageSize, ISectionService sections, CancellationToken ct) =>
            ApiResponse<PaginatedResult<SectionResponse>>.Success(
                await sections.GetPageAsync(page, pageSize, ct)));

        group.MapGet("/{id:int}", async (int id, ISectionService sections, CancellationToken ct) =>
        {
            var section = await sections.GetByIdAsync(id, ct);
            return section is null
                ? ApiResponse.NotFound("Kayıt bulunamadı.")
                : ApiResponse<SectionResponse>.Success(section);
        });

        group.MapPost("", async (CreateSectionRequest request, ISectionService sections, CancellationToken ct) =>
        {
            var id = await sections.CreateAsync(request, ct);
            return ApiResponse<object>.Created($"/api/sections/{id}", new { id });
        });

        group.MapPut("/{id:int}", async (int id, UpdateSectionRequest request, ISectionService sections, CancellationToken ct) =>
        {
            await sections.UpdateAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, ISectionService sections, CancellationToken ct) =>
        {
            await sections.DeleteAsync(id, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
