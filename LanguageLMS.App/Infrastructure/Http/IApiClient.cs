namespace LanguageLMS.App.Infrastructure.Http;

public interface IApiClient
{
    Task<ApiResult<TResponse>> GetAsync<TResponse>(string path, CancellationToken ct = default);

    Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct = default);
}
