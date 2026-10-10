using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LanguageLMS.Core.Common.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private const string DefaultImage = "/images/noPhoto.png";
    private readonly string _root;

    public LocalFileStorage(IWebHostEnvironment env, IConfiguration config)
    {
        _root = StoragePaths.ResolveRoot(env, config);
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
        if (path.StartsWith("http://") || path.StartsWith("https://")) return path;

        return $"{StoragePaths.RequestPath}/{path.TrimStart('/')}";
    }
}
