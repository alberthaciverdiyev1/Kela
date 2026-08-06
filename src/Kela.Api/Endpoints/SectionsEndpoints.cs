using Kela.Application.Sections;

namespace Kela.Api.Endpoints;

public static class SectionsEndpoints
{
    public static IEndpointRouteBuilder MapSectionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sections");

        group.MapGet("", async (int page, int pageSize, ISectionService sections, CancellationToken ct) =>
            Results.Ok(await sections.GetPageAsync(page, pageSize, ct)));

        group.MapGet("/{id:int}", async (int id, ISectionService sections, CancellationToken ct) =>
        {
            var section = await sections.GetByIdAsync(id, ct);
            return section is null ? Results.NotFound() : Results.Ok(section);
        });

        group.MapPost("", async (SectionRequest request, ISectionService sections, CancellationToken ct) =>
        {
            var id = await sections.CreateAsync(request.Name, request.Level, request.TeacherId, ct);
            return Results.Created($"/api/sections/{id}", new { id });
        });

        group.MapPut("/{id:int}", async (int id, SectionRequest request, ISectionService sections, CancellationToken ct) =>
        {
            await sections.UpdateAsync(id, request.Name, request.Level, request.TeacherId, ct);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, ISectionService sections, CancellationToken ct) =>
        {
            await sections.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        return app;
    }

    public sealed record SectionRequest(string Name, int Level, int? TeacherId);
}
