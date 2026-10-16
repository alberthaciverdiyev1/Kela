using Microsoft.AspNetCore.Http;

namespace Kela.Core.Common.Storage;

public interface IFileStorage
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);

    Task DeleteAsync(string? path, CancellationToken ct = default);

    string GetUrl(string? path);

    /// <summary>Birbaşa yükləmə üçün presigned PUT URL. Dəstəklənmirsə null.</summary>
    string? CreatePresignedPutUrl(string path, string contentType, TimeSpan expiry);

    /// <summary>
    /// Birbaşa endirmə üçün presigned GET URL. R2 kimi provayderlərdə worker-a
    /// storage açarı vermədən fayl endirməyə imkan verir. Dəstəklənmirsə null.
    /// </summary>
    string? CreatePresignedGetUrl(string path, TimeSpan expiry);

    /// <summary>Kiçik mətn faylı (məs. m3u8 playlist) yazır.</summary>
    Task UploadTextAsync(string path, string content, string contentType = "application/octet-stream", CancellationToken ct = default);

    /// <summary>Kiçik mətn faylını oxuyur (m3u8 doğrulaması üçün). Yoxdursa null.</summary>
    Task<string?> ReadTextAsync(string path, CancellationToken ct = default);

    /// <summary>Verilmiş prefiks altındakı bütün açarları qaytarır (HLS seqment yoxlaması üçün).</summary>
    Task<IReadOnlyList<string>> ListAsync(string prefix, CancellationToken ct = default);

    /// <summary>Yüklənən faylın həqiqətən mövcud olduğunu yoxlayır.</summary>
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Orijinal fayl adını saxlayaraq boş açar qurur; ad tutulubsa "-1", "-2" artırılır.
    /// İstifadəçi verdiyi adı görmək istədiyi üçün GUID əvəzinə həmin ad saxlanılır.
    /// </summary>
    Task<string> BuildUniquePathAsync(string folder, string fileName, CancellationToken ct = default);

    /// <summary>
    /// Axını birbaşa storage-a yazır. API-nin öz üzərindən yükləmə (proxy) rejimi üçün —
    /// presigned PUT dəstəklənməyəndə worker bu yolla yazır.
    /// </summary>
    Task UploadStreamAsync(string path, Stream stream, string contentType, CancellationToken ct = default);

    // ---- Çoxhissəli yükləmə (böyük fayllar) ----
    // Nazik PUT bir neçə yüz MB-a qədər yararlıdır; GB-larla video üçün hissələrə bölmək
    // lazımdır ki, paralel yüklənsin və kəsiləndə yalnız bir hissə təkrarlansın.

    /// <summary>Çoxhissəli yükləməni başladır. Provayder dəstəkləmirsə null.</summary>
    Task<MultipartTicket?> StartMultipartAsync(string folder, string fileName, string contentType, CancellationToken ct = default);

    /// <summary>Verilmiş hissə nömrələri üçün presigned PUT URL-ləri.</summary>
    IReadOnlyList<MultipartPartUrl> SignParts(string path, string uploadId, IReadOnlyList<int> partNumbers, TimeSpan expiry);

    /// <summary>Hissələri birləşdirib obyekti tamamlayır.</summary>
    Task CompleteMultipartAsync(string path, string uploadId, IReadOnlyList<MultipartPartResult> parts, CancellationToken ct = default);

    /// <summary>Yarımçıq yükləməni ləğv edir — R2 yarımçıq hissələri 7 gün saxlayır və yer tutur.</summary>
    Task AbortMultipartAsync(string path, string uploadId, CancellationToken ct = default);
}
