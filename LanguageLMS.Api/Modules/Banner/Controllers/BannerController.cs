using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Api.Common.Extensions;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Banner.Requests;
using LanguageLMS.Core.Modules.Banner.Responses;
using LanguageLMS.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Banner.Controllers;

// API yalnız app (şagird) üçündür: admin servisi birbaşa inject edir.
// Oxuma student, yazma teacher üçündür.
[Authorize]
[Route("api/[controller]")]
public sealed class BannerController(IBannerService service) : ApiControllerBase
{
    // App: banner siyahısı — paging zərfi olmadan sadə massiv (digər app endpoint-ləri kimi)
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Student)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BannerResponse>>>> List(
        [FromQuery] BannerListRequest request, CancellationToken ct)
    {
        var page = await service.ListAsync(request, ct);
        var data = page.Data.Select(Absolute).ToList();
        return Success<IReadOnlyList<BannerResponse>>(data);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = AuthPolicies.Student)]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Show(long id, CancellationToken ct)
        => Success(Absolute(await service.GetAsync(id, ct)));

    // Şəkil URL-i absolute edir: R2 artıq http(s) qaytarır, local `/uploads/...` üçün
    // cari host önünə əlavə olunur ki, app (başqa origin) şəkli yükləyə bilsin.
    private BannerResponse Absolute(BannerResponse banner)
        => banner with { Image = Request.ToAbsoluteUrl(banner.Image)! };

    [HttpPost]
    [Authorize(Policy = AuthPolicies.Teacher)]
    [HasPermission("banner.manage")]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Add(
        [FromForm] BannerUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("{id:long}")]
    [Authorize(Policy = AuthPolicies.Teacher)]
    [HasPermission("banner.manage")]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Update(
        long id, [FromForm] BannerUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(id, request, ct), "Updated");

    [HttpDelete("{id:long}")]
    [Authorize(Policy = AuthPolicies.Teacher)]
    [HasPermission("banner.manage")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }

    [HttpPost("bulk")]
    [Authorize(Policy = AuthPolicies.Teacher)]
    [HasPermission("banner.manage")]
    public async Task<ActionResult<ApiResponse<string>>> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        await service.BulkAsync(request, ct);
        return SuccessMessage("Bulk action applied");
    }
}
