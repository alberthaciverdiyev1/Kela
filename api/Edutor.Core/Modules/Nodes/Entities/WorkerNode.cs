using Edutor.Core.Common.Entities;
using Edutor.Nodes.Shared.Contracts;

namespace Edutor.Core.Modules.Nodes.Entities;

/// <summary>Konversiya işlərini yerinə yetirən qeydiyyatdan keçmiş cihaz.</summary>
public class WorkerNode : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>bax: <see cref="NodeStatus"/>.</summary>
    public string Status { get; set; } = NodeStatus.Offline;

    /// <summary>Access token-in SHA-256 hash-i — açar özü heç vaxt saxlanılmır.</summary>
    public string? TokenHash { get; set; }

    public DateTimeOffset? TokenIssuedAt { get; set; }

    public DateTimeOffset RegisteredAt { get; set; }

    public DateTimeOffset? LastSeenAt { get; set; }

    /// <summary><see cref="NodeDeviceInfo"/> JSON-u.</summary>
    public string DeviceJson { get; set; } = "{}";

    /// <summary><see cref="NodeRuntimeLimits"/> JSON-u.</summary>
    public string LimitsJson { get; set; } = "{}";

    /// <summary><see cref="NodeBenchmarkResult"/> JSON-u.</summary>
    public string? BenchmarkJson { get; set; }

    /// <summary>Scheduler balı — benchmark və nüvə sayından hesablanır.</summary>
    public int PerformanceScore { get; set; } = 100;

    public int MaxConcurrentJobs { get; set; } = 1;

    public int ActiveJobs { get; set; }

    public long CompletedJobs { get; set; }

    public long FailedJobs { get; set; }

    /// <summary>Admin tərəfindən söndürülə bilər — söndürülmüş cihaz tapşırıq almır.</summary>
    public bool Enabled { get; set; } = true;

    public string? Version { get; set; }
}
