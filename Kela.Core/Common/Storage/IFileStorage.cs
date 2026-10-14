using Microsoft.AspNetCore.Http;

namespace Kela.Core.Common.Storage;

public interface IFileStorage
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);

    Task DeleteAsync(string? path, CancellationToken ct = default);

    string GetUrl(string? path);

    /// <summary>Birbaşa yükləmə üçün presigned PUT URL. Dəstəklənmirsə null.</summary>
    string? CreatePresignedPutUrl(string path, string contentType, TimeSpan expiry);

    /// <summary>Yüklənən faylın həqiqətən mövcud olduğunu yoxlayır.</summary>
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
}
