using LanguageLMS.Core.Modules.Progress.Requests;
using LanguageLMS.Core.Modules.Progress.Responses;

namespace LanguageLMS.Core.Modules.Progress.Services;

public interface IProgressService
{
    Task<MarkProgressResponse> MarkCompletedAsync(long studentId, MarkProgressRequest request, CancellationToken ct = default);

    Task<ProgressResponse> GetProgressAsync(long studentId, CancellationToken ct = default);

    Task<StatisticsResponse> GetStatisticsAsync(long studentId, CancellationToken ct = default);
}
