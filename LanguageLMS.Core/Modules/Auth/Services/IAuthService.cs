using LanguageLMS.Core.Modules.Auth.Requests;
using LanguageLMS.Core.Modules.Auth.Responses;

namespace LanguageLMS.Core.Modules.Auth.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<StudentResponse> GetProfileAsync(long studentId, CancellationToken ct = default);
}
