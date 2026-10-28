namespace Edutor.Core.Common.Storage;

/// <summary>Çoxhissəli yükləmə başladıldı — müştəri hissələri bu məlumatla göndərir.</summary>
/// <param name="Path">Obyektin yekun açarı.</param>
/// <param name="UploadId">S3/R2 yükləmə identifikatoru.</param>
/// <param name="PartSize">Hissə ölçüsü (bayt). R2 son hissə xaric bütün hissələrin bərabər
/// olmasını tələb edir, ona görə ölçü serverdə sabitlənir və müştəri ona əməl edir.</param>
public record MultipartTicket(string Path, string UploadId, int PartSize);

/// <summary>Bir hissə üçün presigned PUT URL.</summary>
public record MultipartPartUrl(int PartNumber, string Url);

/// <summary>Yüklənmiş hissənin nəticəsi — ETag cavab başlığından götürülür.</summary>
public record MultipartPartResult(int PartNumber, string ETag);
