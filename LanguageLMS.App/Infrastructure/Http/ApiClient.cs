using System.Net.Http.Json;

namespace LanguageLMS.App.Infrastructure.Http;

public sealed class ApiClient(HttpClient httpClient) : IApiClient
{
    public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string path, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync(path, ct);
            return await ReadAsync<TResponse>(response, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ApiResult<TResponse>.Failure(ex.Message);
        }
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(path, request, ct);
            return await ReadAsync<TResponse>(response, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ApiResult<TResponse>.Failure(ex.Message);
        }
    }

    private static async Task<ApiResult<TResponse>> ReadAsync<TResponse>(HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return ApiResult<TResponse>.Failure(string.IsNullOrWhiteSpace(error)
                ? $"{(int)response.StatusCode} {response.ReasonPhrase}"
                : error);
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<TResponse>>(cancellationToken: ct);
        if (envelope is null)
        {
            return ApiResult<TResponse>.Failure("API returned an empty response.");
        }

        if (!envelope.Success)
        {
            return ApiResult<TResponse>.Failure(envelope.Message);
        }

        return envelope.Data is null
            ? ApiResult<TResponse>.Failure("API returned an empty response.")
            : ApiResult<TResponse>.Success(envelope.Data);
    }
}
