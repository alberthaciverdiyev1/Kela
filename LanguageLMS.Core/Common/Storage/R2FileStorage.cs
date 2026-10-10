using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LanguageLMS.Core.Common.Storage;

/// <summary>Cloudflare R2 (S3-uyğun) üzərində fayl/video saxlayır.</summary>
public sealed class R2FileStorage : IFileStorage, IDisposable
{
    private const string DefaultImage = "/images/noPhoto.png";
    private readonly R2Options _options;
    private readonly IAmazonS3 _client;

    public R2FileStorage(IOptions<R2Options> options)
    {
        _options = options.Value;

        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{_options.AccountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true
        };

        _client = new AmazonS3Client(_options.AccessKeyId, _options.SecretAccessKey, config);
    }

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{folder}-{Guid.NewGuid():N}{extension}";
        var path = $"{folder}/{fileName}";

        await using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            InputStream = stream,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            DisablePayloadSigning = true // R2 üçün
        };

        await _client.PutObjectAsync(request, ct);
        return path;
    }

    public async Task DeleteAsync(string? path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path) || path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await _client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = path
        }, ct);
    }

    public string GetUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return DefaultImage;
        if (path.StartsWith("http://") || path.StartsWith("https://")) return path;

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    public void Dispose() => _client.Dispose();
}
