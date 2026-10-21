using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Sorting;
using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.Lesson.Responses;

namespace Kela.Core.Modules.Lesson.Abstractions;

public interface ILessonService
{
    Task<PagedResponse<LessonResponse>> ListAsync(LessonListRequest request, CancellationToken ct = default);

    /// <summary><paramref name="ownerId"/> verilərsə yalnız öz/admin dərsinə giriş icazəsi verilir.</summary>
    Task<LessonResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<LessonResponse> CreateAsync(LessonUpsertRequest request, CancellationToken ct = default);

    Task<LessonResponse> UpdateAsync(long id, LessonUpsertRequest request, long? ownerId = null, CancellationToken ct = default);

    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task SetActiveAsync(long id, bool isActive, long? ownerId = null, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default);

    Task ReorderAsync(SortRequest request, long? ownerId = null, CancellationToken ct = default);

    Task<LessonSettingsResponse> GetSettingsAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<LessonSettingsResponse> SetSettingsAsync(long id, SetLessonSettingsRequest request, long? ownerId = null, CancellationToken ct = default);
}
