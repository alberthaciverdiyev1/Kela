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
        // Şagird tətbiqi yalnız aktiv və müddəti bitməmiş bannerləri görür.
        request.OnlyActive = true;
        return Success((await service.ListAsync(request, ct)).Data);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));
}
