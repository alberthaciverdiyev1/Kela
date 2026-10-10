using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Dashboard.Abstractions;
using LanguageLMS.Core.Modules.Dashboard.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Dashboard.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[Route("api/[controller]")]
public sealed class DashboardController(IDashboardService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> Get(CancellationToken ct)
        => Success(await service.GetAsync(ct));
}
