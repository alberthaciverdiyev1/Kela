using Kela.Core.Modules.Progress.Requests;
using Kela.Core.Modules.Progress.Responses;

namespace Kela.Core.Modules.Progress.Abstractions;

public interface IProgressService
{
    Task<MarkProgressResponse> MarkCompletedAsync(long studentId, MarkProgressRequest request, CancellationToken ct = default);

    Task<ProgressResponse> GetProgressAsync(long studentId, CancellationToken ct = default);

    Task<StatisticsResponse> GetStatisticsAsync(long studentId, CancellationToken ct = default);
}
