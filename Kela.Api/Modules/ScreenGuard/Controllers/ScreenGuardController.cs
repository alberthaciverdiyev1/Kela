using System.Security.Claims;
using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.ScreenGuard.Abstractions;
using Kela.Core.Modules.ScreenGuard.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Modules.ScreenGuard.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class ScreenGuardController(IScreenGuardService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Şagird tərəfində aşkarlanan ekran qoruması pozuntusunu qeydə alır.</summary>
    [HttpPost("violations")]
    public async Task<ActionResult<ApiResponse<string>>> Log(
        [FromBody] LogScreenViolationRequest request, CancellationToken ct)
    {
        await service.LogAsync(StudentId, request, Request.Headers.UserAgent.ToString(), ct);
        return SuccessMessage("Logged");
    }
}
