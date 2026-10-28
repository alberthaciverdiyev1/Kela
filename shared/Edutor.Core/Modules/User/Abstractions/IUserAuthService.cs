using Edutor.Core.Modules.User.Requests;
using Edutor.Core.Modules.User.Responses;

namespace Edutor.Core.Modules.User.Abstractions;

public interface IUserAuthService
{
    Task<UserAuthResponse> LoginAsync(UserLoginRequest request, CancellationToken ct = default);

    Task<UserResponse> GetProfileAsync(long userId, CancellationToken ct = default);
}
