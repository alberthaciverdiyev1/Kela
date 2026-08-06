using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Responses;

namespace Kela.Application.Features.Users.Auth;

public interface IAuthService
{
    /// <summary>Geçersiz kimlik bilgisi → null döner (endpoint 401'e çevirir).</summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
