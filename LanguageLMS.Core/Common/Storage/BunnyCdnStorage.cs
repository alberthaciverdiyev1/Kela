using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LanguageLMS.Core.Common.Storage;

public sealed class BunnyCdnStorage(HttpClient http, IOptions<BunnyCdnOptions> options) : IFileStorage
{
    private const string DefaultImage = "/images/noPhoto.png";
    private readonly BunnyCdnOptions _options = options.Value;

    private string StorageHost => string.IsNullOrWhiteSpace(_options.Region)
        ? "storage.bunnycdn.com"
        : $"{_options.Region}.storage.bunnycdn.com";

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{folder}-{Guid.NewGuid():N}{extension}";
        var path = $"{folder}/{fileName}";

        using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var request = new HttpRequestMessage(
            HttpMethod.Put, $"https://{StorageHost}/{_options.StorageZone}/{path}")
        {
            Content = content
        };
        request.Headers.Add("AccessKey", _options.ApiKey);

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return path;
    }

    public async Task DeleteAsync(string? path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path) || path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Delete, $"https://{StorageHost}/{_options.StorageZone}/{path}");
        request.Headers.Add("AccessKey", _options.ApiKey);

        using var response = await http.SendAsync(request, ct);
    }

    public string GetUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return DefaultImage;
        if (path.StartsWith("http://") || path.StartsWith("https://")) return path;

        var pullZone = _options.PullZone.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? _options.PullZone
            : $"https://{_options.PullZone}";

        return $"{pullZone.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    public string? CreatePresignedPutUrl(string path, string contentType, TimeSpan expiry) => null;

    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        try
        {
            using var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, GetUrl(path)), ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
