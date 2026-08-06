using Kela.Api.Contracts;
using Kela.Application.Features.SiteConfiguration;
using Kela.Application.Features.SiteConfiguration.Requests;
using Kela.Application.Features.SiteConfiguration.Responses;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

/// <summary>
/// TEK uç: site geneli konfigürasyon.
///   GET /api/site-config  → tüm ayarlar (giriş yapan herkes)
///   PUT /api/site-config  → tüm ayarları güncelle (yalnızca Admin/Teacher)
/// Yeni bir ayar eklemek için buraya yeni uç yazmaya gerek yok —
/// entity + request/response'a alan eklemek yeterli.
/// </summary>
public static class SiteConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapSiteConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/site-config");

        group.MapGet("", async (ISiteConfigurationService config, CancellationToken ct) =>
            ApiResponse<SiteConfigurationResponse>.Success(await config.GetAsync(ct)))
            .RequireAuthorization();

        group.MapPut("", async (
            UpdateSiteConfigurationRequest request, ISiteConfigurationService config, CancellationToken ct) =>
        {
            await config.UpdateAsync(request, ct);
            return ApiResponse.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole(RoleNames.Admin, RoleNames.Teacher));

        return app;
    }
}
