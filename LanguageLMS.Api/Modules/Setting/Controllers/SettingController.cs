using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Setting.Requests;
using LanguageLMS.Core.Modules.Setting.Responses;
using LanguageLMS.Core.Modules.Setting.Services;
using LanguageLMS.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Setting.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[HasPermission("setting.manage")]
[Route("api/[controller]")]
public sealed class SettingController(ISettingService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Get(CancellationToken ct)
        => Success(await service.GetAsync(ct));

    [HttpPut]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Update(
        [FromForm] SettingUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(request, ct), "Updated");

    [AllowAnonymous]
    [HttpGet("app-version")]
    public async Task<ActionResult<ApiResponse<AppVersionResponse>>> AppVersion(CancellationToken ct)
        => Success(await service.GetAppVersionAsync(ct));
}
