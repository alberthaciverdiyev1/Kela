using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Setting.Requests;
using LanguageLMS.Core.Modules.Setting.Responses;
using LanguageLMS.Core.Modules.Setting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Setting.Controllers;

public sealed class SettingController(ISettingService service) : ApiControllerBase
{
    [HttpGet("api/setting")]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Get(CancellationToken ct)
        => Success(await service.GetAsync(ct));

    [HttpPut("api/setting")]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Update(
        [FromForm] SettingUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(request, ct), "Updated");

    [AllowAnonymous]
    [HttpGet("api/app-version")]
    public async Task<ActionResult<ApiResponse<AppVersionResponse>>> AppVersion(CancellationToken ct)
        => Success(await service.GetAppVersionAsync(ct));
}
