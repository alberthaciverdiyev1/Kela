using Kela.Api.Contracts;
using Kela.Application.Features.Cities;
using Kela.Application.Features.Cities.Requests;
using Kela.Application.Features.Cities.Responses;
using Kela.Application.Pagination;
using Kela.Domain.Common;
using Microsoft.AspNetCore.Builder;

namespace Kela.Api.Endpoints;

/// <summary>
/// Şehirler — referans verisi (çevirili).
/// Okuma (GET) giriş yapan herkese açıktır (öğretmen formlarında şehir seçimi
/// için). Yazma (POST/PUT/DELETE) yalnızca Admin'i aittir.
/// Tüm uçlar isteğe bağlı <c>lang</c> (az/en/ru/tr) alır; yanıt o dildeki
/// adla döner (dil yoksa en → az → ilk mevcut ad sırasıyla düşer).
///   GET    /api/cities?lang=tr&page=1&pageSize=10 → sayfalı liste (yerelleştirilmiş adlar)
///   GET    /api/cities/1?lang=en                    → detay + tüm dillerdeki adlar
///   POST   /api/cities                              → 4 dilde ad alır (az/en/ru/tr)  [Admin]
///   PUT    /api/cities/1                            → 4 dildeki adları günceller       [Admin]
///   DELETE /api/cities/1                            → şehri + çevirilerini siler       [Admin]
/// </summary>
public static class CitiesEndpoints
{
    public static IEndpointRouteBuilder MapCitiesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cities");

        // Okuma — giriş yapan herkes (dropdown/referans verisi).
        group.MapGet("", async (int page, int pageSize, string? lang, ICityService cities, CancellationToken ct) =>
            ApiResponse<PaginatedResult<CityListItemResponse>>.Success(
                await cities.GetPageAsync(page, pageSize, lang, ct)))
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
