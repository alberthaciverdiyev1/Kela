using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Banner.Requests;
using LanguageLMS.Core.Modules.Banner.Responses;
using LanguageLMS.Core.Modules.Banner.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Banner.Controllers;

public sealed class BannerController(IBannerService service) : ApiControllerBase
{
    [HttpGet("api/banner")]
    public async Task<ActionResult<ApiResponse<PagedResponse<BannerResponse>>>> List(
        [FromQuery] BannerListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("api/banner/{id:long}")]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost("api/banner")]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Add(
        [FromForm] BannerUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("api/banner/{id:long}")]
    public async Task<ActionResult<ApiResponse<BannerResponse>>> Update(
        long id, [FromForm] BannerUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(id, request, ct), "Updated");

    [HttpDelete("api/banner/{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }
}
