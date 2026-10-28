using Microsoft.AspNetCore.Http;
using Edutor.Core.Modules.Auth.Requests;
using Edutor.Core.Modules.Auth.Responses;

namespace Edutor.Core.Modules.Auth.Abstractions;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<StudentResponse> GetProfileAsync(long studentId, CancellationToken ct = default);

    Task SendOtpAsync(SendOtpRequest request, CancellationToken ct = default);

    Task<bool> CheckOtpAsync(CheckOtpRequest request, CancellationToken ct = default);

    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);

    Task ChangePasswordAsync(long studentId, ChangePasswordRequest request, CancellationToken ct = default);

    Task<StudentResponse> ChangeEmailAsync(long studentId, ChangeEmailRequest request, CancellationToken ct = default);

    Task<StudentResponse> ChangePhoneAsync(long studentId, ChangePhoneRequest request, CancellationToken ct = default);

    Task<StudentResponse> ChangeFullNameAsync(long studentId, ChangeFullNameRequest request, CancellationToken ct = default);

    Task<StudentResponse> ChangeImageAsync(long studentId, IFormFile file, CancellationToken ct = default);

    Task<StudentResponse> SetNotificationStatusAsync(long studentId, NotificationStatusRequest request, CancellationToken ct = default);

    Task DeleteAccountAsync(long studentId, CancellationToken ct = default);
}
