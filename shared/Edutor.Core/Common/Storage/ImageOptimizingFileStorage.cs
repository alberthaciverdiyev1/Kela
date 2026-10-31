using Microsoft.AspNetCore.Http;

namespace Edutor.Core.Common.Storage;

/// <summary>
/// Bütün host-larda şəkil yükləmələri eyni yoldan keçsin deyə storage dekore edilir:
/// iş mantığı (banner, logo, şagird şəkli, cavab şəkli...) optimallaşdırmadan xəbərsizdir.
/// </summary>
public sealed class ImageOptimizingFileStorage(IFileStorage inner, ImageOptimizer optimizer) : IFileStorage
{
    public IFileStorage Inner => inner;

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
        => await inner.UploadAsync(await optimizer.OptimizeAsync(file, ct), folder, ct);

    public Task DeleteAsync(string? path, CancellationToken ct = default) => inner.DeleteAsync(path, ct);

    public string GetUrl(string? path) => inner.GetUrl(path);

    public string? CreatePresignedPutUrl(string path, string contentType, TimeSpan expiry)
        => inner.CreatePresignedPutUrl(path, contentType, expiry);

    public string? CreatePresignedGetUrl(string path, TimeSpan expiry) => inner.CreatePresignedGetUrl(path, expiry);

    public Task UploadTextAsync(string path, string content, string contentType = "application/octet-stream", CancellationToken ct = default)
        => inner.UploadTextAsync(path, content, contentType, ct);

    public Task<string?> ReadTextAsync(string path, CancellationToken ct = default) => inner.ReadTextAsync(path, ct);

    public Task<IReadOnlyList<string>> ListAsync(string prefix, CancellationToken ct = default) => inner.ListAsync(prefix, ct);

    public Task<IReadOnlyList<StoredObject>> ListObjectsAsync(string prefix, CancellationToken ct = default)
        => inner.ListObjectsAsync(prefix, ct);

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default) => inner.ExistsAsync(path, ct);

    public Task<long?> GetSizeAsync(string path, CancellationToken ct = default) => inner.GetSizeAsync(path, ct);

    public Task<string> BuildUniquePathAsync(string folder, string fileName, CancellationToken ct = default)
        => inner.BuildUniquePathAsync(folder, fileName, ct);

    public Task UploadStreamAsync(string path, Stream stream, string contentType, CancellationToken ct = default)
        => inner.UploadStreamAsync(path, stream, contentType, ct);

    public Task<MultipartTicket?> StartMultipartAsync(string folder, string fileName, string contentType, CancellationToken ct = default)
        => inner.StartMultipartAsync(folder, fileName, contentType, ct);

    public IReadOnlyList<MultipartPartUrl> SignParts(string path, string uploadId, IReadOnlyList<int> partNumbers, TimeSpan expiry)
        => inner.SignParts(path, uploadId, partNumbers, expiry);

    public Task CompleteMultipartAsync(string path, string uploadId, IReadOnlyList<MultipartPartResult> parts, CancellationToken ct = default)
        => inner.CompleteMultipartAsync(path, uploadId, parts, ct);

    public Task AbortMultipartAsync(string path, string uploadId, CancellationToken ct = default)
        => inner.AbortMultipartAsync(path, uploadId, ct);
}
