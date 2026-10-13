using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Setting.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Setting.Controllers;

// App yalnız versiya yoxlaması üçün istifadə edir. Tənzimləmələr admin MVC-dədir.
[Route("api/[controller]")]
public sealed class SettingController(ISettingService service) : ApiControllerBase
{
    [HttpGet("contact")]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Contact(CancellationToken ct)
        => Success(await service.GetAsync(ct));

    [HttpGet("app-version")]
    public async Task<ActionResult<ApiResponse<AppVersionResponse>>> AppVersion(CancellationToken ct)
        => Success(await service.GetAppVersionAsync(ct));
}
