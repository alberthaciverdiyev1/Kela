using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Extensions;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Banner.Requests;
using LanguageLMS.Core.Modules.Banner.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Banner.Controllers;

// API yalnız app (şagird) üçündür — banner oxunur, idarəetmə admin MVC-dədir.
[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class BannerController(IBannerService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BannerResponse>>>> List(
        [FromQuery] BannerListRequest request, CancellationToken ct)
    {
        var page = await service.ListAsync(request, ct);
        var data = page.Data.Select(Absolute).ToList();
        return Success<IReadOnlyList<BannerResponse>>(data);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Show(long id, CancellationToken ct)
        => Success(Absolute(await service.GetAsync(id, ct)));

    // Şəkil URL-i absolute edir: R2 artıq http(s) qaytarır, local `/uploads/...` üçün
    // cari host önünə əlavə olunur ki, app (başqa origin) şəkli yükləyə bilsin.
    private BannerResponse Absolute(BannerResponse banner)
        => banner with { Image = Request.ToAbsoluteUrl(banner.Image)! };
}
