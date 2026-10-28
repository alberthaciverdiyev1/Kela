using Edutor.Core.Common.Storage;
using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Responses;
using Edutor.Core.Common.Sorting;
using Edutor.Core.Modules.LessonContent.Requests;
using Edutor.Core.Modules.LessonContent.Responses;

namespace Edutor.Core.Modules.LessonContent.Abstractions;

public interface ILessonContentService
{
    Task<PagedResponse<LessonContentResponse>> ListAsync(LessonContentListRequest request, CancellationToken ct = default);

    /// <summary><paramref name="ownerId"/> verilərsə yalnız öz/admin materialına giriş icazəsi verilir.</summary>
    Task<LessonContentResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default);

    /// <summary>Presigned PUT URL — client faylı birbaşa R2-yə yükləyir, sonra Path ilə CreateAsync çağırır.</summary>
    Task<UploadUrlResponse> CreateUploadUrlAsync(LessonContentUploadUrlRequest request, long? ownerId = null, CancellationToken ct = default);

    // ---- Böyük fayllar üçün çoxhissəli yükləmə ----

    /// <summary>Çoxhissəli yükləməni başladır; provayder dəstəkləmirsə null.</summary>
    Task<MultipartTicket?> StartMultipartAsync(LessonContentUploadUrlRequest request, long? ownerId = null, CancellationToken ct = default);

    /// <summary>Hissələr üçün presigned URL-lər.</summary>
    IReadOnlyList<MultipartPartUrl> SignParts(MultipartPartsRequest request);

    Task CompleteMultipartAsync(MultipartCompleteRequest request, CancellationToken ct = default);

    Task AbortMultipartAsync(MultipartAbortRequest request, CancellationToken ct = default);

    Task<LessonContentResponse> CreateAsync(LessonContentCreateRequest request, CancellationToken ct = default);

    /// <summary>Fon rejimində yüklənmiş faylı mövcud materiala bağlayır.</summary>
    Task<LessonContentResponse> AttachFileAsync(long id, LessonContentAttachRequest request, long? ownerId = null, CancellationToken ct = default);

    Task<LessonContentResponse> UpdateAsync(long id, LessonContentUpdateRequest request, long? ownerId = null, CancellationToken ct = default);

    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task SetActiveAsync(long id, bool isActive, long? ownerId = null, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default);

    Task ReorderAsync(SortRequest request, long? ownerId = null, CancellationToken ct = default);
}
