using Microsoft.AspNetCore.Http;

namespace LanguageLMS.Core.Common.Storage;

public interface IFileStorage
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);

    Task DeleteAsync(string? path, CancellationToken ct = default);

    string GetUrl(string? path);
}
