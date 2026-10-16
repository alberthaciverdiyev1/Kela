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

    /// <summary>
    /// Orijinal fayl adını saxlayaraq boş açar qurur; ad tutulubsa "-1", "-2" artırılır.
    /// İstifadəçi verdiyi adı görmək istədiyi üçün GUID əvəzinə həmin ad saxlanılır.
    /// </summary>
    Task<string> BuildUniquePathAsync(string folder, string fileName, CancellationToken ct = default);
}
