using System.Security.Cryptography;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Common.Responses;
using Kela.Core.Data;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Core.Modules.Nodes.Entities;
using Kela.Core.Modules.Nodes.Requests;
using Kela.Core.Modules.Nodes.Responses;
using Kela.Core.Modules.Media.Entities;
using Kela.Nodes.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kela.Core.Modules.Nodes.Services;

using WorkerNode = Kela.Core.Modules.Nodes.Entities.WorkerNode;

public sealed class NodeService(
    AppDbContext db,
    ITenantScope tenantScope,
    IOptions<NodeOptions> options) : INodeService
{
    private static readonly string[] WorkerStages =
        [JobStatus.Assigned, JobStatus.Downloading, JobStatus.Processing, JobStatus.Uploading];

    private NodeOptions Settings => options.Value;

    public async Task<NodeRegisterResponse> RegisterAsync(NodeRegisterRequest request, CancellationToken ct = default)
    {
        var code = request.RegistrationCode?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Registration code is required");
        }

        // Qeydiyyat anonim sorğudur — tenant hələ bilinmir, ona görə filtrlər söndürülür
        // və kod sətrinin özü tenant-ı müəyyən edir.
        var record = await db.WorkerRegistrationCodes.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == code && x.DeletedAt == null, ct);

        if (record is null) throw new UnauthorizedAccessException("Invalid registration code");
        if (record.UsedAt is not null) throw new InvalidOperationException("Registration code has already been used");
        if (record.ExpiresAt is not null && record.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Registration code has expired");
        }

        tenantScope.Override = record.TenantId;

        var token = NodeTokens.Create();
        var now = DateTimeOffset.UtcNow;

        var node = new WorkerNode
        {
            Name = Truncate(request.Name, 120) is { Length: > 0 } name ? name : $"node-{now:yyyyMMddHHmmss}",
            Status = NodeStatus.Online,
            TokenHash = NodeTokens.Hash(token),
            TokenIssuedAt = now,
            RegisteredAt = now,
            LastSeenAt = now,
            DeviceJson = NodeJson.Write(request.Device),
            LimitsJson = NodeJson.Write(new NodeRuntimeLimits()),
            MaxConcurrentJobs = 1,
            Enabled = true,
            TenantId = record.TenantId
        };

        db.WorkerNodes.Add(node);
        await db.SaveChangesAsync(ct);

        record.UsedAt = now;
        record.UsedByNodeId = node.Id;
        await db.SaveChangesAsync(ct);

        return new NodeRegisterResponse
        {
            NodeId = node.Id,
            AccessToken = token,
            HeartbeatSeconds = Settings.HeartbeatSeconds,
            PollSeconds = Settings.PollSeconds,
            MaxConcurrentJobs = node.MaxConcurrentJobs
        };
    }

    public async Task<WorkerNode?> AuthenticateAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var hash = NodeTokens.Hash(token);

        // Worker sorğusunda JWT yoxdur → tenant filtri söndürülür, cihaz token ilə tapılır.
        var node = await db.WorkerNodes.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TokenHash == hash && x.DeletedAt == null, ct);

        if (node is null || !node.Enabled || node.Status == NodeStatus.Disabled) return null;

        tenantScope.Override = node.TenantId;
        return node;
    }

    public async Task<NodeHeartbeatResponse> HeartbeatAsync(
        WorkerNode node, NodeHeartbeatRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        node.LastSeenAt = now;
        node.Status = node.Enabled ? NodeStatus.Online : NodeStatus.Disabled;

        if (request.Device is not null) node.DeviceJson = NodeJson.Write(request.Device);

        if (request.Limits is not null)
        {
            node.LimitsJson = NodeJson.Write(request.Limits);
            // 0 = cihaz öz profilinə buraxıb; o halda mövcud dəyər saxlanılır.
            if (request.Limits.MaxConcurrentJobs > 0)
            {
                node.MaxConcurrentJobs = Math.Clamp(request.Limits.MaxConcurrentJobs, 1, Settings.MaxConcurrentJobsPerNode);
            }
        }

        if (request.Benchmark is not null)
        {
            node.BenchmarkJson = NodeJson.Write(request.Benchmark);
            node.PerformanceScore = Math.Clamp(request.Benchmark.Score, 1, 10_000);
        }

        var reported = request.Jobs.ToDictionary(j => j.JobId);

        var serverJobs = await db.VideoJobs.IgnoreQueryFilters()
            .Where(j => j.WorkerNodeId == node.Id && j.DeletedAt == null)
            .ToListAsync(ct);

        foreach (var job in serverJobs)
        {
            if (reported.TryGetValue(job.Id, out var report) && job.AttemptToken == report.AttemptToken
                && !JobStatus.IsTerminal(job.Status))
            {
                job.Progress = Math.Clamp(report.Progress, 0, 1);

                // Yalnız worker mərhələləri qəbul olunur: "completed" yalnız
                // CompleteAsync (çıxışların yoxlanılması) ilə təyin edilə bilər.
                if (WorkerStages.Contains(report.Status)) job.Status = report.Status;
                if (!string.IsNullOrWhiteSpace(report.Message)) job.ErrorMessage = Truncate(report.Message, 1000);

                job.LeaseExpiresAt = now.AddMinutes(Settings.LeaseMinutes);
            }
        }

        await db.SaveChangesAsync(ct);

        var activeIds = serverJobs.Where(j => JobStatus.IsActive(j.Status)).Select(j => j.Id).ToHashSet();
        var cancelIds = reported.Keys.Where(id => !activeIds.Contains(id)).ToList();

        node.ActiveJobs = activeIds.Count;
        await db.SaveChangesAsync(ct);

        return new NodeHeartbeatResponse
        {
            ServerTime = now,
            ActiveJobIds = [.. activeIds],
            CancelJobIds = cancelIds,
            HeartbeatSeconds = Settings.HeartbeatSeconds,
            PollSeconds = Settings.PollSeconds
        };
    }

    public async Task<NodeRotateTokenResponse> RotateTokenAsync(WorkerNode node, CancellationToken ct = default)
    {
        var token = NodeTokens.Create();

        node.TokenHash = NodeTokens.Hash(token);
        node.TokenIssuedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return new NodeRotateTokenResponse { AccessToken = token };
    }

    public async Task<PagedResponse<WorkerNodeResponse>> ListAsync(NodeListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 500 ? 50 : request.PerPage;

        var query = db.WorkerNodes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Name, term));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.LastSeenAt ?? x.RegisteredAt)
            .Skip((page - 1) * perPage).Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<WorkerNodeResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<WorkerNodeResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var node = await db.WorkerNodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Worker node not found: {id}");

        return ToResponse(node);
    }

    public async Task<WorkerNodeResponse> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var node = await db.WorkerNodes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Worker node not found: {id}");

        node.Enabled = enabled;
        node.Status = enabled ? NodeStatus.Offline : NodeStatus.Disabled;

        if (!enabled)
        {
            node.TokenHash = null;
            node.ActiveJobs = 0;
        }

        await db.SaveChangesAsync(ct);
        return ToResponse(node);
    }

    public async Task RevokeTokenAsync(long id, CancellationToken ct = default)
    {
        var node = await db.WorkerNodes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Worker node not found: {id}");

        // Token silinir; cihaz yenidən qoşulmaq üçün yeni qeydiyyat kodu tələb edir.
        node.TokenHash = null;
        node.Status = NodeStatus.Disabled;
        node.Enabled = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var node = await db.WorkerNodes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Worker node not found: {id}");

        node.DeletedAt = DateTimeOffset.UtcNow;
        node.TokenHash = null;
        node.Status = NodeStatus.Disabled;
        await db.SaveChangesAsync(ct);
    }

    public async Task<RegistrationCodeResponse> CreateCodeAsync(
        CreateRegistrationCodeRequest request, CancellationToken ct = default)
    {
        var code = GenerateCode();
        var expires = request.ExpiresHours > 0
            ? DateTimeOffset.UtcNow.AddHours(Math.Min(request.ExpiresHours, 24 * 30))
            : (DateTimeOffset?)null;

        var record = new WorkerRegistrationCode
        {
            Code = code,
            Note = Truncate(request.Note, 190),
            ExpiresAt = expires
        };

        db.WorkerRegistrationCodes.Add(record);
        await db.SaveChangesAsync(ct);

        return new RegistrationCodeResponse(record.Id, record.Code, record.Note, record.ExpiresAt,
            null, null, null, record.CreatedAt);
    }

    public async Task<IReadOnlyList<RegistrationCodeResponse>> ListCodesAsync(CancellationToken ct = default)
    {
        var codes = await db.WorkerRegistrationCodes.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(200)
            .ToListAsync(ct);

        var nodeIds = codes.Where(x => x.UsedByNodeId is not null).Select(x => x.UsedByNodeId!.Value).ToList();

        var names = await db.WorkerNodes.IgnoreQueryFilters()
            .Where(n => nodeIds.Contains(n.Id))
            .Select(n => new { n.Id, n.Name })
            .ToDictionaryAsync(n => n.Id, n => n.Name, ct);

        return codes.Select(x => new RegistrationCodeResponse(
            x.Id, x.Code, x.Note, x.ExpiresAt, x.UsedAt, x.UsedByNodeId,
            x.UsedByNodeId is { } id && names.TryGetValue(id, out var name) ? name : null,
            x.CreatedAt)).ToList();
    }

    public async Task DeleteCodeAsync(long id, CancellationToken ct = default)
    {
        var record = await db.WorkerRegistrationCodes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Registration code not found: {id}");

        record.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<NodeSummaryResponse> SummaryAsync(CancellationToken ct = default)
    {
        var nodes = await db.WorkerNodes.AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var jobs = await db.VideoJobs.IgnoreQueryFilters().GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var videos = await db.Videos.IgnoreQueryFilters()
            .Where(v => v.DeletedAt == null)
            .Where(v => v.ConversionStatus == Media.Entities.VideoStatus.Pending
                     || v.ConversionStatus == Media.Entities.VideoStatus.Processing)
            .Select(v => v.ConversionStatus)
            .ToListAsync(ct);

        int Count(string status) => jobs.FirstOrDefault(j => j.Status == status)?.Count ?? 0;

        return new NodeSummaryResponse(
            TotalNodes: nodes.Sum(n => n.Count),
            OnlineNodes: nodes.FirstOrDefault(n => n.Status == NodeStatus.Online)?.Count ?? 0,
            OfflineNodes: nodes.FirstOrDefault(n => n.Status == NodeStatus.Offline)?.Count ?? 0,
            DisabledNodes: nodes.FirstOrDefault(n => n.Status == NodeStatus.Disabled)?.Count ?? 0,
            ActiveJobs: jobs.Where(j => JobStatus.IsActive(j.Status)).Sum(j => j.Count),
            PendingJobs: Count(JobStatus.Pending),
            CompletedJobs: Count(JobStatus.Completed),
            FailedJobs: Count(JobStatus.Failed),
            RunningVideos: videos.Count(v => v == Media.Entities.VideoStatus.Processing),
            PendingVideos: videos.Count(v => v == Media.Entities.VideoStatus.Pending));
    }

    public async Task<PagedResponse<VideoJobResponse>> ListJobsAsync(
        VideoJobListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 500 ? 50 : request.PerPage;

        var query = db.VideoJobs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status);
        }

        if (request.VideoId is { } videoId)
        {
            query = query.Where(x => x.VideoId == videoId);
        }

        if (request.NodeId is { } nodeId)
        {
            query = query.Where(x => x.WorkerNodeId == nodeId);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage).Take(perPage)
            .Select(x => new VideoJobResponse(
                x.Id,
                x.VideoId,
                x.Video!.Title,
                x.Resolution,
                x.TargetHeight,
                x.TargetBitrateKbps,
                x.Status,
                x.Progress,
                x.Attempts,
                x.MaxAttempts,
                x.WorkerNodeId,
                x.WorkerNode!.Name,
                x.LeaseExpiresAt,
                x.StartedAt,
                x.CompletedAt,
                x.ErrorMessage,
                x.ErrorKind,
                x.Skipped,
                x.EncodingFps,
                x.Encoder,
                x.CreatedAt))
            .ToListAsync(ct);

        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<VideoJobResponse>(items, page, lastPage, perPage, total);
    }

    public async Task RequeueJobAsync(long jobId, CancellationToken ct = default)
    {
        var job = await db.VideoJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct)
            ?? throw new KeyNotFoundException($"Video job not found: {jobId}");

        job.Status = JobStatus.Pending;
        job.Attempts = 0;
        job.Progress = 0;
        job.ErrorMessage = null;
        job.ErrorKind = null;
        job.WorkerNodeId = null;
        job.AttemptToken = null;
        job.LeaseExpiresAt = null;
        job.NextAttemptAt = null;
        job.StartedAt = null;
        job.CompletedAt = null;
        job.Skipped = false;

        // Video yenidən emala salınır; köhnə uğurlu versiya yayımda qalır.
        var video = await db.Videos.FirstOrDefaultAsync(v => v.Id == job.VideoId, ct);
        if (video is not null) video.ConversionStatus = VideoStatus.Processing;

        await db.SaveChangesAsync(ct);
    }

    private static WorkerNodeResponse ToResponse(WorkerNode node) => new(
        node.Id,
        node.Name,
        node.Status,
        node.Enabled,
        node.Version,
        NodeJson.Device(node.DeviceJson),
        NodeJson.Limits(node.LimitsJson),
        NodeJson.Benchmark(node.BenchmarkJson),
        node.PerformanceScore,
        node.MaxConcurrentJobs,
        node.ActiveJobs,
        node.CompletedJobs,
        node.FailedJobs,
        node.RegisteredAt,
        node.LastSeenAt,
        node.TokenIssuedAt,
        !string.IsNullOrWhiteSpace(node.TokenHash));

    /// <summary>Qarışdırıcı simvollar çıxarılır ki, kod telefonda əl ilə yazıla bilsin.</summary>
    private static string GenerateCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var chars = new char[16];

        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        }

        return $"{new string(chars, 0, 4)}-{new string(chars, 4, 4)}-{new string(chars, 8, 4)}-{new string(chars, 12, 4)}";
    }

    private static string? Truncate(string? value, int max)
        => string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value.Trim() : value[..max].Trim();
}
