using Kela.Api.Contracts;
using Kela.Application.Features.Cities;
using Kela.Application.Features.Cities.Requests;
using Kela.Application.Features.Cities.Responses;
using Kela.Application.Pagination;
using Microsoft.AspNetCore.Builder;

namespace Kela.Api.Endpoints;

/// <summary>
/// Şehir CRUD — yalnızca Admin erişebilir.
/// Tüm uçlar isteğe bağlı <c>lang</c> (az/en/ru/tr) alır; yanıt o dildeki
/// adla döner (dil yoksa en → az → ilk mevcut ad sırasıyla düşer).
///   GET    /api/cities?lang=tr&page=1&pageSize=10 → sayfalı liste (yerelleştirilmiş adlar)
///   GET    /api/cities/1?lang=en                    → detay + tüm dillerdeki adlar
///   POST   /api/cities                              → 4 dilde ad alır (az/en/ru/tr)
///   PUT    /api/cities/1                            → 4 dildeki adları günceller
///   DELETE /api/cities/1                            → şehri + çevirilerini siler
/// </summary>
public static class CitiesEndpoints
{
    public static IEndpointRouteBuilder MapCitiesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cities")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("", async (int page, int pageSize, string? lang, ICityService cities, CancellationToken ct) =>
            ApiResponse<PaginatedResult<CityListItemResponse>>.Success(
                await cities.GetPageAsync(page, pageSize, lang, ct)));

        group.MapGet("/{id:int}", async (int id, string? lang, ICityService cities, CancellationToken ct) =>
        {
            var city = await cities.GetByIdAsync(id, lang, ct);
            return city is null
                ? ApiResponse.NotFound("Şehir bulunamadı.")
                : ApiResponse<CityResponse>.Success(city);
        });

        group.MapPost("", async (CreateCityRequest request, ICityService cities, CancellationToken ct) =>
        {
            var id = await cities.CreateAsync(request, ct);
            return ApiResponse<object>.Created($"/api/cities/{id}", new { id });
        });

        group.MapPut("/{id:int}", async (int id, UpdateCityRequest request, ICityService cities, CancellationToken ct) =>
        {
            await cities.UpdateAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, ICityService cities, CancellationToken ct) =>
        {
            await cities.DeleteAsync(id, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
