using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Lesson.Requests;
using LanguageLMS.Core.Modules.Lesson.Responses;

namespace LanguageLMS.Core.Modules.Lesson.Services;

public interface ILessonService
{
    Task<PagedResponse<LessonResponse>> ListAsync(LessonListRequest request, CancellationToken ct = default);

    Task<LessonResponse> GetAsync(long id, CancellationToken ct = default);

    Task<LessonResponse> CreateAsync(LessonUpsertRequest request, CancellationToken ct = default);

    Task<LessonResponse> UpdateAsync(long id, LessonUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
