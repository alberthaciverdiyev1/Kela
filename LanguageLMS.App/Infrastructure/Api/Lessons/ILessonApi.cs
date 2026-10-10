using LanguageLMS.App.Infrastructure.Http;

namespace LanguageLMS.App.Infrastructure.Api.Lessons;

public interface ILessonApi
{
    Task<ApiResult<PagedResponse<LessonListItem>>> ListAsync(LessonListQuery query, CancellationToken ct = default);
}
