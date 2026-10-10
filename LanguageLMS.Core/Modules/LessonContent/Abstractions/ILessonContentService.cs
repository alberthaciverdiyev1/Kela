using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.LessonContent.Requests;
using LanguageLMS.Core.Modules.LessonContent.Responses;

namespace LanguageLMS.Core.Modules.LessonContent.Abstractions;

public interface ILessonContentService
{
    Task<PagedResponse<LessonContentResponse>> ListAsync(LessonContentListRequest request, CancellationToken ct = default);

    Task<LessonContentResponse> GetAsync(long id, CancellationToken ct = default);

    /// <summary>Presigned PUT URL — client faylı birbaşa R2-yə yükləyir, sonra Path ilə CreateAsync çağırır.</summary>
    Task<UploadUrlResponse> CreateUploadUrlAsync(LessonContentUploadUrlRequest request, CancellationToken ct = default);

    Task<LessonContentResponse> CreateAsync(LessonContentCreateRequest request, CancellationToken ct = default);

    Task<LessonContentResponse> UpdateAsync(long id, LessonContentUpdateRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
