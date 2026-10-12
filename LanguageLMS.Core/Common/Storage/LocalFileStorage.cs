using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LanguageLMS.Core.Common.Storage;

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
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{folder}-{Guid.NewGuid():N}{extension}";

        var directory = Path.Combine(_root, folder);
        Directory.CreateDirectory(directory);

        await using var stream = new FileStream(Path.Combine(directory, fileName), FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return $"{folder}/{fileName}";
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
