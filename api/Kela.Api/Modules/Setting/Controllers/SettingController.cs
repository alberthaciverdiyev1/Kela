using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Modules.Setting.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Modules.Setting.Controllers;

// App yalnız versiya yoxlaması üçün istifadə edir. Tənzimləmələr admin MVC-dədir.
[Route("api/[controller]")]
public sealed class SettingController(ISettingService service) : ApiControllerBase
{
    [HttpGet("contact")]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Contact(CancellationToken ct)
        => Success(await service.GetAsync(ct));

    /// <summary>Platforma (superadmin) əlaqə məlumatları — tenant-dan asılı olmayan.</summary>
    [HttpGet("platform")]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Platform(CancellationToken ct)
        => Success(await service.GetPlatformAsync(ct));

    [HttpGet("app-version")]
    public async Task<ActionResult<ApiResponse<AppVersionResponse>>> AppVersion(CancellationToken ct)
        => Success(await service.GetAppVersionAsync(ct));
}
