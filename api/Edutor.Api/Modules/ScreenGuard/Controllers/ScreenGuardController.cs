using System.Security.Claims;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.ScreenGuard.Abstractions;
using Edutor.Core.Modules.ScreenGuard.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Edutor.Api.Modules.ScreenGuard.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class ScreenGuardController(IScreenGuardService service, IStringLocalizer<SharedResource> L) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Şagird tərəfində aşkarlanan ekran qoruması pozuntusunu qeydə alır.</summary>
    [HttpPost("violations")]
    public async Task<ActionResult<ApiResponse<string>>> Log(
        [FromBody] LogScreenViolationRequest request, CancellationToken ct)
    {
        await service.LogAsync(StudentId, request, Request.Headers.UserAgent.ToString(), ct);
        return SuccessMessage(L["Api_Logged"]);
    }
}
