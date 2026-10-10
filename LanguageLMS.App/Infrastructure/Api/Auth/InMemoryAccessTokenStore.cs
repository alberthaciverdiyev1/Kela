namespace LanguageLMS.App.Infrastructure.Api.Auth;

public sealed class InMemoryAccessTokenStore : IAccessTokenStore
{
    private string? _token;

    public Task<string?> GetAsync(CancellationToken ct = default) => Task.FromResult(_token);

    public Task SetAsync(string? token, CancellationToken ct = default)
    {
        _token = token;
        return Task.CompletedTask;
    }
}
