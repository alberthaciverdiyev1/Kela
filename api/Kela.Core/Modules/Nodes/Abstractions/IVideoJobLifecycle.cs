using Kela.Core.Modules.Nodes.Entities;
using Kela.Nodes.Shared.Contracts;

namespace Kela.Core.Modules.Nodes.Abstractions;

/// <summary>Worker-ın tapşırıq üzrə bildirişləri və nəticənin doğrulanması.</summary>
public interface IVideoJobLifecycle
{
    Task<JobOperationResult> ReportProgressAsync(WorkerNode node, long jobId, JobProgressRequest request, CancellationToken ct = default);

    Task<(JobOperationResult Result, JobUploadUrlResponse? Urls)> CreateUploadUrlsAsync(
        WorkerNode node, long jobId, JobUploadUrlRequest request, CancellationToken ct = default);

    Task<JobOperationResult> CompleteAsync(WorkerNode node, long jobId, JobCompleteRequest request, CancellationToken ct = default);

    Task<JobOperationResult> FailAsync(WorkerNode node, long jobId, JobFailRequest request, CancellationToken ct = default);

    /// <summary>Bütün pillələr bitibsə master playlist-i yazır və videonu yayıma açır.</summary>
    Task<JobOperationResult> FinalizeAsync(long videoId, CancellationToken ct = default);
}
