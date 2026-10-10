using LanguageLMS.App.Infrastructure.Api.Auth;

namespace LanguageLMS.App.Infrastructure.Http;

public sealed class AuthHeaderHandler(IAccessTokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
