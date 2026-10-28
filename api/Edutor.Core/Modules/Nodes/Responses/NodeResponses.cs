using Edutor.Nodes.Shared.Contracts;

namespace Edutor.Core.Modules.Nodes.Responses;

public record WorkerNodeResponse(
    long Id,
    string Name,
    string Status,
    bool Enabled,
    string? Version,
    NodeDeviceInfo Device,
    NodeRuntimeLimits Limits,
    NodeBenchmarkResult? Benchmark,
    int PerformanceScore,
    int MaxConcurrentJobs,
    int ActiveJobs,
    long CompletedJobs,
    long FailedJobs,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset? TokenIssuedAt,
    bool HasToken);

public record RegistrationCodeResponse(
    long Id,
    string Code,
    string? Note,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? UsedAt,
    long? UsedByNodeId,
    string? UsedByNodeName,
    DateTimeOffset CreatedAt)
{
    public bool IsUsed => UsedAt is not null;

    public bool IsExpired => ExpiresAt is not null && ExpiresAt < DateTimeOffset.UtcNow;
}

public record VideoJobResponse(
    long Id,
    long VideoId,
    string? VideoTitle,
    string Resolution,
    int TargetHeight,
    int TargetBitrateKbps,
    string Status,
    double Progress,
    int Attempts,
    int MaxAttempts,
    long? NodeId,
    string? NodeName,
    DateTimeOffset? LeaseExpiresAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorMessage,
    string? ErrorKind,
    bool Skipped,
    double? EncodingFps,
    string? Encoder,
    DateTimeOffset CreatedAt);

public record NodeSummaryResponse(
    int TotalNodes,
    int OnlineNodes,
    int OfflineNodes,
    int DisabledNodes,
    int ActiveJobs,
    int PendingJobs,
    int CompletedJobs,
    int FailedJobs,
    int RunningVideos,
    int PendingVideos);
