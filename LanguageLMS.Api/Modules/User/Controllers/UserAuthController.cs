using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.User.Requests;
using LanguageLMS.Core.Modules.User.Responses;
using LanguageLMS.Core.Modules.User.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.User.Controllers;

[Route("api/[controller]")]
public sealed class UserAuthController(IUserAuthService service) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserAuthResponse>>> Login(
        [FromBody] UserLoginRequest request, CancellationToken ct)
        => Success(await service.LoginAsync(request, ct), "Login successful");

    [Authorize(Policy = AuthPolicies.Teacher)]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Me(CancellationToken ct)
    {
        var id = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Success(await service.GetProfileAsync(id, ct));
    }

    [Authorize(Policy = AuthPolicies.Teacher)]
    [HttpPost("logout")]
    public ActionResult<ApiResponse<string>> Logout() => SuccessMessage("Logged out");
}
