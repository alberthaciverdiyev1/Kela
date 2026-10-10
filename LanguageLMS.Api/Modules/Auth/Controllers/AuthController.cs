using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth.Requests;
using LanguageLMS.Core.Modules.Auth.Responses;
using LanguageLMS.Core.Modules.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Auth.Controllers;

public sealed class AuthController(IAuthService service) : ApiControllerBase
{
    [HttpPost("api/auth/register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
        => Success(await service.RegisterAsync(request, ct), "Registered", StatusCodes.Status201Created);

    [HttpPost("api/auth/login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
        => Success(await service.LoginAsync(request, ct), "Login successful");

    [Authorize]
    [HttpGet("api/auth/me")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Me(CancellationToken ct)
    {
        var id = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Success(await service.GetProfileAsync(id, ct));
    }

    [Authorize]
    [HttpPost("api/auth/logout")]
    public ActionResult<ApiResponse<string>> Logout() => SuccessMessage("Logged out");
}
