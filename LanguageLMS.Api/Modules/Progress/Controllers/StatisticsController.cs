using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Progress.Responses;
using LanguageLMS.Core.Modules.Progress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Progress.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StatisticsController(IProgressService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<StatisticsResponse>>> Get(CancellationToken ct)
        => Success(await service.GetStatisticsAsync(StudentId, ct));
}
