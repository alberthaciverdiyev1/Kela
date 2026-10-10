using LanguageLMS.App.Infrastructure.Http;

namespace LanguageLMS.App.Infrastructure.Api.Auth;

public interface IAuthApi
{
    Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
