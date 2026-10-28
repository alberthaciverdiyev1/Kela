using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Edutor.Core.Common.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private const string DefaultImage = "/images/noPhoto.png";
    private readonly string _root;
    private readonly IHttpContextAccessor _accessor;

    public LocalFileStorage(IWebHostEnvironment env, IConfiguration config, IHttpContextAccessor accessor)
    {
        _root = StoragePaths.ResolveRoot(env, config);
        _accessor = accessor;
    }

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var path = await BuildUniquePathAsync(folder, file.FileName, ct);
        var fullPath = Resolve(path);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return path;
    }

    public Task DeleteAsync(string? path, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            var fullPath = Resolve(path);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GetUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return DefaultImage;
        if (IsAbsolute(path)) return path;

        return Absolute($"{StoragePaths.RequestPath}/{path.TrimStart('/')}");
    }

    public string? CreatePresignedPutUrl(string path, string contentType, TimeSpan expiry) => null;

    public string? CreatePresignedGetUrl(string path, TimeSpan expiry) => null;

    public async Task UploadStreamAsync(string path, Stream stream, string contentType, CancellationToken ct = default)
    {
        var fullPath = Resolve(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var file = new FileStream(fullPath, FileMode.Create);
        await stream.CopyToAsync(file, ct);
    }

    public async Task UploadTextAsync(string path, string content, string contentType = "application/octet-stream", CancellationToken ct = default)
    {
        var fullPath = Resolve(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8, ct);
    }

    public async Task<string?> ReadTextAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Resolve(path);
        return File.Exists(fullPath) ? await File.ReadAllTextAsync(fullPath, Encoding.UTF8, ct) : null;
    }

    public Task<IReadOnlyList<string>> ListAsync(string prefix, CancellationToken ct = default)
    {
        var root = Resolve(prefix);
        if (!Directory.Exists(root)) return Task.FromResult<IReadOnlyList<string>>([]);

        var keys = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(_root, f).Replace('\\', '/'))
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(keys);
    }

    public Task<IReadOnlyList<StoredObject>> ListObjectsAsync(string prefix, CancellationToken ct = default)
    {
        var root = Resolve(prefix);
        if (!Directory.Exists(root)) return Task.FromResult<IReadOnlyList<StoredObject>>([]);

        var objects = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(file =>
            {
                var info = new FileInfo(file);
                return new StoredObject(
                    Path.GetRelativePath(_root, file).Replace('\\', '/'),
                    info.Length,
                    info.LastWriteTimeUtc);
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<StoredObject>>(objects);
    }

    public Task<string> BuildUniquePathAsync(string folder, string fileName, CancellationToken ct = default)
    {
        var name = StorageNaming.Sanitize(fileName);
        var path = $"{folder}/{name}";

        var i = 1;
        while (File.Exists(Resolve(path)))
        {
            var stem = Path.GetFileNameWithoutExtension(name);
            var ext = Path.GetExtension(name);
            path = $"{folder}/{stem}-{i++}{ext}";
        }

        return Task.FromResult(path);
    }

    // Lokal diskdə çoxhissəli yükləməyə ehtiyac yoxdur — provayder dəstəkləmir.
    public Task<MultipartTicket?> StartMultipartAsync(string folder, string fileName, string contentType, CancellationToken ct = default)
        => Task.FromResult<MultipartTicket?>(null);

    public IReadOnlyList<MultipartPartUrl> SignParts(string path, string uploadId, IReadOnlyList<int> partNumbers, TimeSpan expiry)
        => [];

    public Task CompleteMultipartAsync(string path, string uploadId, IReadOnlyList<MultipartPartResult> parts, CancellationToken ct = default)
        => throw new NotSupportedException("Multipart upload is not supported by local storage");

    public Task AbortMultipartAsync(string path, string uploadId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
        => Task.FromResult(File.Exists(Resolve(path)));

    public Task<long?> GetSizeAsync(string path, CancellationToken ct = default)
    {
        var full = Resolve(path);
        var info = new FileInfo(full);

        return Task.FromResult(info.Exists ? (long?)info.Length : null);
    }

    private static bool IsAbsolute(string url)
        => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
           || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Bucket açarını disk yoluna çevirir və kökdən kənara çıxmasının qarşısını alır
    /// (açar xarici worker-dan gəlir → "../" ilə hüduddan çıxma cəhdi mümkündür).
    /// </summary>
    private string Resolve(string key)
    {
        var full = Path.GetFullPath(Path.Combine(_root, key.Replace('\\', '/').TrimStart('/')));
        var root = Path.GetFullPath(_root);

        if (full != root && !full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Invalid storage key: {key}");
        }

        return full;
    }

    /// <summary>Nisbi yolu cari sorğunun sxem+host-u ilə absolute edir (R2 ilə eyni davranış).</summary>
    private string Absolute(string relativePath)
    {
        var ctx = _accessor.HttpContext;
        return ctx is null
            ? relativePath
            : $"{ctx.Request.Scheme}://{ctx.Request.Host}{relativePath}";
    }
}
