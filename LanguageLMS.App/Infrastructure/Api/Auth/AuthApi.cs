using LanguageLMS.App.Infrastructure.Http;

namespace LanguageLMS.App.Infrastructure.Api.Auth;

public sealed class AuthApi(IApiClient apiClient, IAccessTokenStore tokenStore) : IAuthApi
{
    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var result = await apiClient.PostAsync<LoginRequest, LoginResponse>("api/user-auth/login", request, ct);
        if (result.IsSuccess)
        {
            await tokenStore.SetAsync(result.Value?.Token, ct);
        }

        return result;
    }
}
