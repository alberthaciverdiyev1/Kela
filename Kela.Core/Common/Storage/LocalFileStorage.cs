using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Kela.Core.Common.Storage;

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
        var fileName = Path.GetFileName(path);

        var directory = Path.Combine(_root, folder);
        Directory.CreateDirectory(directory);

        await using var stream = new FileStream(Path.Combine(directory, fileName), FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return path;
    }

    public Task DeleteAsync(string? path, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            var fullPath = Path.Combine(_root, path);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
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

    public Task<string> BuildUniquePathAsync(string folder, string fileName, CancellationToken ct = default)
    {
        var name = StorageNaming.Sanitize(fileName);
        var path = $"{folder}/{name}";

        var i = 1;
        while (File.Exists(Path.Combine(_root, path)))
        {
            var stem = Path.GetFileNameWithoutExtension(name);
            var ext = Path.GetExtension(name);
            path = $"{folder}/{stem}-{i++}{ext}";
        }

        return Task.FromResult(path);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
        => Task.FromResult(File.Exists(Path.Combine(_root, path)));

    private static bool IsAbsolute(string url)
        => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
           || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    /// <summary>Nisbi yolu cari sorğunun sxem+host-u ilə absolute edir (R2 ilə eyni davranış).</summary>
    private string Absolute(string relativePath)
    {
        var ctx = _accessor.HttpContext;
        return ctx is null
            ? relativePath
            : $"{ctx.Request.Scheme}://{ctx.Request.Host}{relativePath}";
    }
}
