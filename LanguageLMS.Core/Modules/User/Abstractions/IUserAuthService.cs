using LanguageLMS.Core.Modules.User.Requests;
using LanguageLMS.Core.Modules.User.Responses;

namespace LanguageLMS.Core.Modules.User.Abstractions;

public interface IUserAuthService
{
    Task<UserAuthResponse> LoginAsync(UserLoginRequest request, CancellationToken ct = default);

    Task<UserResponse> GetProfileAsync(long userId, CancellationToken ct = default);
}
