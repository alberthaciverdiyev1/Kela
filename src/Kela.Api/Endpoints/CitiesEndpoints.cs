using Kela.Api.Contracts;
using Kela.Application.Features.Cities;
using Kela.Application.Features.Cities.Requests;
using Kela.Application.Features.Cities.Responses;
using Kela.Application.Pagination;
using Kela.Domain.Common;
using Microsoft.AspNetCore.Builder;

namespace Kela.Api.Endpoints;

public static class CitiesEndpoints
{
    public static IEndpointRouteBuilder MapCitiesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cities");

        // Okuma — giriş yapan herkes (dropdown/referans verisi).
        group.MapGet("", async (int page, string? lang, ICityService cities, CancellationToken ct) =>
            ApiResponse<PaginatedResult<CityListItemResponse>>.Success(
                await cities.GetPageAsync(page, lang, ct)))
            .RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, string? lang, ICityService cities, CancellationToken ct) =>
        {
            var city = await cities.GetByIdAsync(id, lang, ct);
            return city is null
                ? ApiResponse.NotFound("Şehir bulunamadı.")
                : ApiResponse<CityResponse>.Success(city);
        })
        .RequireAuthorization();

        // Yazma — yalnızca Admin.
        group.MapPost("", async (CreateCityRequest request, ICityService cities, CancellationToken ct) =>
        {
            var id = await cities.CreateAsync(request, ct);
            return ApiResponse<object>.Created($"/api/cities/{id}", new { id });
        })
        .RequireAuthorization(policy => policy.RequireRole(RoleNames.Admin));

        group.MapPut("/{id:int}", async (int id, UpdateCityRequest request, ICityService cities, CancellationToken ct) =>
        {
            await cities.UpdateAsync(id, request, ct);
            return ApiResponse.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole(RoleNames.Admin));

        group.MapDelete("/{id:int}", async (int id, ICityService cities, CancellationToken ct) =>
        {
            await cities.DeleteAsync(id, ct);
            return ApiResponse.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole(RoleNames.Admin));

        return app;
    }
}
