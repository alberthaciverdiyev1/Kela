using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Responses;
using Kela.Application.Features.Users.Requests;

namespace Kela.Application.Features.Users.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<int> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
