namespace LanguageLMS.App.Infrastructure.Api.Auth;

public interface IAccessTokenStore
{
    Task<string?> GetAsync(CancellationToken ct = default);

    Task SetAsync(string? token, CancellationToken ct = default);
}
