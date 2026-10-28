using System.Net;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Edutor.Core.Common.Storage;

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
        var path = await BuildUniquePathAsync(folder, file.FileName, ct);

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

    public string? CreatePresignedPutUrl(string path, string contentType, TimeSpan expiry)
        => _client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiry),
            ContentType = contentType
        });

    public string? CreatePresignedGetUrl(string path, TimeSpan expiry)
        => _client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry)
        });

    public async Task UploadStreamAsync(string path, Stream stream, string contentType, CancellationToken ct = default)
    {
        await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            InputStream = stream,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            DisablePayloadSigning = true
        }, ct);
    }

    public async Task UploadTextAsync(string path, string content, string contentType = "application/octet-stream", CancellationToken ct = default)
    {
        var bytes = Encoding.UTF8.GetBytes(content);

        await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            InputStream = new MemoryStream(bytes),
            ContentType = contentType,
            DisablePayloadSigning = true
        }, ct);
    }

    public async Task<string?> ReadTextAsync(string path, CancellationToken ct = default)
    {
        try
        {
            using var response = await _client.GetObjectAsync(_options.BucketName, path, ct);
            using var reader = new StreamReader(response.ResponseStream, Encoding.UTF8);
            return await reader.ReadToEndAsync(ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<string>> ListAsync(string prefix, CancellationToken ct = default)
    {
        var keys = new List<string>();
        string? token = null;

        do
        {
            var response = await _client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _options.BucketName,
                Prefix = prefix,
                ContinuationToken = token
            }, ct);

            keys.AddRange(response.S3Objects.Select(o => o.Key));
            token = response.IsTruncated == true ? response.NextContinuationToken : null;
        }
        while (token is not null);

        return keys;
    }

    public async Task<IReadOnlyList<StoredObject>> ListObjectsAsync(string prefix, CancellationToken ct = default)
    {
        var objects = new List<StoredObject>();
        string? token = null;

        do
        {
            var response = await _client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
            {
                BucketName = _options.BucketName,
                Prefix = prefix,
                ContinuationToken = token
            }, ct);

            objects.AddRange(response.S3Objects.Select(o => new StoredObject(
                o.Key,
                o.Size ?? 0,
                new DateTimeOffset(o.LastModified?.ToUniversalTime() ?? DateTime.UtcNow, TimeSpan.Zero))));

            token = response.IsTruncated == true ? response.NextContinuationToken : null;
        }
        while (token is not null);

        return objects;
    }

    public async Task<string> BuildUniquePathAsync(string folder, string fileName, CancellationToken ct = default)
    {
        var name = StorageNaming.Sanitize(fileName);
        var path = $"{folder}/{name}";

        var i = 1;
        while (await ExistsAsync(path, ct))
        {
            var stem = Path.GetFileNameWithoutExtension(name);
            var ext = Path.GetExtension(name);
            path = $"{folder}/{stem}-{i++}{ext}";
        }

        return path;
    }

    // R2 son hissə xaric bütün hissələrin bərabər olmasını tələb edir; ölçü sabitdir.
    private const int MultipartPartSize = 16 * 1024 * 1024;

    public async Task<MultipartTicket?> StartMultipartAsync(string folder, string fileName, string contentType, CancellationToken ct = default)
    {
        var path = await BuildUniquePathAsync(folder, fileName, ct);

        var response = await _client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
        }, ct);

        return new MultipartTicket(path, response.UploadId, MultipartPartSize);
    }

    public IReadOnlyList<MultipartPartUrl> SignParts(string path, string uploadId, IReadOnlyList<int> partNumbers, TimeSpan expiry)
        => partNumbers
            .Select(n => new MultipartPartUrl(n, _client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = path,
                Verb = HttpVerb.PUT,
                UploadId = uploadId,
                PartNumber = n,
                Expires = DateTime.UtcNow.Add(expiry)
            })))
            .ToList();

    public async Task CompleteMultipartAsync(string path, string uploadId, IReadOnlyList<MultipartPartResult> parts, CancellationToken ct = default)
    {
        await _client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            UploadId = uploadId,
            PartETags = parts
                .OrderBy(p => p.PartNumber)
                .Select(p => new PartETag(p.PartNumber, p.ETag.Trim('"')))
                .ToList()
        }, ct);
    }

    public Task AbortMultipartAsync(string path, string uploadId, CancellationToken ct = default)
        => _client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            UploadId = uploadId
        }, ct);

    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        try
        {
            await _client.GetObjectMetadataAsync(_options.BucketName, path, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<long?> GetSizeAsync(string path, CancellationToken ct = default)
    {
        try
        {
            // HEAD sorğusu — faylın gövdəsi endirilmir, yalnız ölçüsü oxunur.
            var metadata = await _client.GetObjectMetadataAsync(_options.BucketName, path, ct);

            return metadata.ContentLength;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public void Dispose() => _client.Dispose();
}
