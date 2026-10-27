using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Banner.Requests;
using Kela.Core.Modules.Banner.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Modules.Banner.Controllers;

// API yalnız app (şagird) üçündür — banner oxunur, idarəetmə admin MVC-dədir.
// Şəkil URL-ləri absolute-dir (storage.GetUrl → IFileStorage servisi həll edir).
[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class BannerController(IBannerService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BannerResponse>>>> List(
        [FromQuery] BannerListRequest request, CancellationToken ct)
    {
        // Şagird tətbiqi yalnız aktiv və müddəti bitməmiş bannerləri görür;
        // müəllim story-si yalnız öz qruplarına, admin story-si bütün tenant-a, superadmin story-si hamıya.
        request.OnlyActive = true;
        return Success((await service.ListForStudentAsync(CurrentUserId(), request, ct)).Data);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetForStudentAsync(CurrentUserId(), id, ct));

    private long CurrentUserId()
        => long.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
}
