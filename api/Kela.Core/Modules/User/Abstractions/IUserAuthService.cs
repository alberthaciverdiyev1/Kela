using Kela.Core.Modules.User.Requests;
using Kela.Core.Modules.User.Responses;

namespace Kela.Core.Modules.User.Abstractions;

public interface IUserAuthService
{
    Task<UserAuthResponse> LoginAsync(UserLoginRequest request, CancellationToken ct = default);

    Task<UserResponse> GetProfileAsync(long userId, CancellationToken ct = default);
}
