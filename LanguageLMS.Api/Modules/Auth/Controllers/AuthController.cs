using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Auth.Requests;
using LanguageLMS.Core.Modules.Auth.Responses;
using LanguageLMS.Core.Modules.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LanguageLMS.Api.Modules.Auth.Controllers;

[EnableRateLimiting("auth")]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // --- Auth ---

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request, CancellationToken ct)
        => Success(await service.RegisterAsync(request, ct), "Registered", StatusCodes.Status201Created);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
        => Success(await service.LoginAsync(request, ct), "Login successful");

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Me(CancellationToken ct)
        => Success(await service.GetProfileAsync(StudentId, ct));

    // Bir cihazda çoxlu hesab: başqa hesaba keçid → həmin hesabın tokeni qaytarılır
    [AllowAnonymous]
    [HttpPost("switch-account")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> SwitchAccount([FromBody] LoginRequest request, CancellationToken ct)
        => Success(await service.LoginAsync(request, ct), "Login successful");

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpPost("logout")]
    public ActionResult<ApiResponse<string>> Logout() => SuccessMessage("Logged out");

    // --- OTP ---

    [AllowAnonymous]
    [HttpPost("send-otp")]
    public async Task<ActionResult<ApiResponse<string>>> SendOtp([FromBody] SendOtpRequest request, CancellationToken ct)
    {
        await service.SendOtpAsync(request, ct);
        return SuccessMessage("OTP sent");
    }

    [AllowAnonymous]
    [HttpPost("check-otp")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckOtp([FromBody] CheckOtpRequest request, CancellationToken ct)
        => Success(await service.CheckOtpAsync(request, ct), "OTP checked");

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await service.ResetPasswordAsync(request, ct);
        return SuccessMessage("Password reset successfully");
    }

    // --- Hesab idarəsi ---

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await service.ChangePasswordAsync(StudentId, request, ct);
        return SuccessMessage("Password changed");
    }

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpPut("change-email")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> ChangeEmail([FromBody] ChangeEmailRequest request, CancellationToken ct)
        => Success(await service.ChangeEmailAsync(StudentId, request, ct), "Email changed");

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpPut("change-phone")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> ChangePhone([FromBody] ChangePhoneRequest request, CancellationToken ct)
        => Success(await service.ChangePhoneAsync(StudentId, request, ct), "Phone changed");

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpPost("change-image")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> ChangeImage(IFormFile file, CancellationToken ct)
        => Success(await service.ChangeImageAsync(StudentId, file, ct), "Image changed");

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpPut("change-full-name")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> ChangeFullName([FromBody] ChangeFullNameRequest request, CancellationToken ct)
        => Success(await service.ChangeFullNameAsync(StudentId, request, ct), "Name changed");

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpPut("notification-status")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> NotificationStatus([FromBody] NotificationStatusRequest request, CancellationToken ct)
        => Success(await service.SetNotificationStatusAsync(StudentId, request, ct), "Notification status updated");

    [Authorize(Policy = AuthPolicies.Student)]
    [HttpDelete("delete-account")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteAccount(CancellationToken ct)
    {
        await service.DeleteAccountAsync(StudentId, ct);
        return SuccessMessage("Account deleted");
    }
}
