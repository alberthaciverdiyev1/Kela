using System.Security.Claims;
using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Common.Localization;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.User.Requests;
using Kela.Core.Modules.User.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

namespace Kela.Api.Modules.User.Controllers;

[EnableRateLimiting("auth")]
[Route("api/user-auth")]
public sealed class UserAuthController(IUserAuthService service, IStringLocalizer<SharedResource> L) : ApiControllerBase
{
    private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserAuthResponse>>> Login([FromBody] UserLoginRequest request, CancellationToken ct)
        => Success(await service.LoginAsync(request, ct), L["Api_LoginSuccessful"]);

    [Authorize(Policy = AuthPolicies.Teacher)]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Me(CancellationToken ct)
        => Success(await service.GetProfileAsync(UserId, ct));
}
