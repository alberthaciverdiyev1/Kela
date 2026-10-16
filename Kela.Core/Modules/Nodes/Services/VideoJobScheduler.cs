using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Media.Entities;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Nodes.Shared.Contracts;
using Kela.Nodes.Shared.Encoding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kela.Core.Modules.Nodes.Services;

using VideoJob = Kela.Core.Modules.Nodes.Entities.VideoJob;
using WorkerNode = Kela.Core.Modules.Nodes.Entities.WorkerNode;

public sealed class VideoJobScheduler(
    AppDbContext db,
    IFileStorage storage,
    ITenantFolder folders,
    IOptions<NodeOptions> options,
    ILogger<VideoJobScheduler> logger) : IVideoJobScheduler
{
    private NodeOptions Settings => options.Value;

    public async Task EnsureJobsAsync(long videoId, CancellationToken ct = default)
    {
        // Fon xidməti tenant konteksti olmadan işləyir — bütün tenantlar üzrə axtarılır,
        // tenant sərhədi aşağıda video.TenantId ilə açıq şəkildə qorunur.
        var video = await db.Videos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == videoId && x.DeletedAt == null, ct);

        if (video is null || string.IsNullOrWhiteSpace(video.Path)) return;

        var existing = await db.VideoJobs.IgnoreQueryFilters()
            .Where(x => x.VideoId == videoId)
            .ToListAsync(ct);

        var known = existing.Select(x => x.Resolution).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sourceHeight = existing.FirstOrDefault(x => x.SourceHeight.HasValue)?.SourceHeight;

        var profiles = VideoLadder.ForSource(Settings.Ladder, sourceHeight);
        var tenantFolder = await folders.ResolveAsync(video.TenantId, ct);
        var prefix = NodePaths.OutputPrefix(video.Path, video.Id, video.Title, video.ConversionVersion, tenantFolder);

        var created = new List<VideoJob>();

        foreach (var profile in profiles)
        {
            if (known.Contains(profile.Name)) continue;

            created.Add(new VideoJob
            {
                VideoId = video.Id,
                Resolution = profile.Name,
                TargetHeight = profile.Height,
                TargetBitrateKbps = profile.VideoBitrateKbps,
                Status = JobStatus.Pending,
                MaxAttempts = Settings.MaxAttempts,
                OutputPrefix = prefix,
                VariantKey = NodePaths.VariantKey(prefix, profile.Name),
                SourceHeight = sourceHeight,
                Priority = profile.Height,
                TenantId = video.TenantId
            });
        }

        if (created.Count == 0) return;

        db.VideoJobs.AddRange(created);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Paralel işləyən başqa API nüsxəsi eyni tapşırıqları yaratdı — bu normaldır.
            db.ChangeTracker.Clear();
        }
    }

    public async Task ReapAsync(CancellationToken ct = default)
    {
        var reaped = await db.Database.ExecuteSqlRawAsync("""
            UPDATE video_jobs
            SET status = CASE WHEN attempts >= max_attempts THEN 'failed' ELSE 'pending' END,
                error_message = CASE WHEN attempts >= max_attempts
                                     THEN 'Lease expired — attempts exhausted'
                                     ELSE 'Lease expired' END,
                error_kind = 'transient',
                completed_at = CASE WHEN attempts >= max_attempts THEN now() ELSE NULL END,
                next_attempt_at = CASE WHEN attempts >= max_attempts THEN NULL
                                       ELSE now() + (interval '10 second' * power(2, least(attempts, 6))) END,
                worker_node_id = NULL,
                attempt_token = NULL,
                lease_expires_at = NULL,
                progress = 0,
                updated_at = now()
            WHERE deleted_at IS NULL
              AND status = ANY({0})
              AND lease_expires_at IS NOT NULL
              AND lease_expires_at < now()
            """, [JobStatus.Reclaimable]);

        if (reaped > 0) logger.LogWarning("Reclaimed {Count} expired video jobs", reaped);

        // Cihazların aktiv iş sayını həqiqi vəziyyətdən yenidən hesabla.
        await db.Database.ExecuteSqlRawAsync("""
            UPDATE worker_nodes n
            SET active_jobs = (
                SELECT count(*) FROM video_jobs j
                WHERE j.worker_node_id = n.id AND j.deleted_at IS NULL AND j.status = ANY({0})
            )
            WHERE n.deleted_at IS NULL
            """, [JobStatus.Active]);

        var cutoff = DateTimeOffset.UtcNow.AddSeconds(-Settings.OfflineAfterSeconds);

        await db.Database.ExecuteSqlRawAsync("""
            UPDATE worker_nodes
            SET status = 'offline', updated_at = now()
            WHERE deleted_at IS NULL AND status = 'online'
              AND (last_seen_at IS NULL OR last_seen_at < {0})
            """, cutoff);
    }

    public async Task<IReadOnlyList<long>> ReconcileAsync(CancellationToken ct = default)
    {
        var active = JobStatus.Active;

        // 1) İşi yarıda qalmış videolar — aktiv tapşırığı yoxdursa yenidən yaradılır.
        var orphanVideoIds = await db.Videos.IgnoreQueryFilters()
            .Where(v => v.DeletedAt == null)
            .Where(v => v.ConversionStatus == VideoStatus.Pending || v.ConversionStatus == VideoStatus.Processing)
            .Where(v => !db.VideoJobs.IgnoreQueryFilters().Any(j => j.VideoId == v.Id && active.Contains(j.Status)))
            .OrderBy(v => v.Id)
            .Select(v => v.Id)
            .Take(50)
            .ToListAsync(ct);

        foreach (var videoId in orphanVideoIds)
        {
            await EnsureJobsAsync(videoId, ct);
        }

        // 2) Bütün tapşırıqları bitmiş videolar — master playlist yazılmalıdır.
        return await db.Videos.IgnoreQueryFilters()
            .Where(v => v.DeletedAt == null)
            .Where(v => v.ConversionStatus == VideoStatus.Pending || v.ConversionStatus == VideoStatus.Processing)
            .Where(v => db.VideoJobs.IgnoreQueryFilters().Any(j => j.VideoId == v.Id))
            .Where(v => !db.VideoJobs.IgnoreQueryFilters().Any(j => j.VideoId == v.Id && active.Contains(j.Status)))
            .OrderBy(v => v.Id)
            .Select(v => v.Id)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<JobAssignment>> ClaimAsync(
        WorkerNode node, int max, long? preferVideoId = null, CancellationToken ct = default)
    {
        var capacity = Math.Min(node.MaxConcurrentJobs, Settings.MaxConcurrentJobsPerNode) - node.ActiveJobs;
        var take = Math.Min(max, capacity);
        if (take <= 0) return [];

        var lease = DateTimeOffset.UtcNow.AddMinutes(Settings.LeaseMinutes);
        List<VideoJob> jobs;

        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            // Cihaz konkret bir tenant-a bağlanmır: registrasiya kodu tenant-scoped olsa da,
            // worker sadəcə icra qurğusudur və hansı tenant-ın videosu olsa da işləyə bilər.
            // İzolyasiya iş səviyyəsindədir — hər cihaz yalnız özünə verilmiş tapşırığı görür.
            //
            // FOR UPDATE SKIP LOCKED: paralel işləyən API nüsxələri eyni tapşırığı götürə bilməz.
            jobs = await db.VideoJobs
                .FromSql($"""
                    SELECT * FROM video_jobs
                    WHERE deleted_at IS NULL
                      AND status = ANY({new[] { JobStatus.Pending, JobStatus.Queued }})
                      AND (next_attempt_at IS NULL OR next_attempt_at <= now())
                    -- Əvvəlcə ən köhnə video bitirilir, sonra növbəti. Beləliklə
                    -- bir videonun bütün pillələri paralel paylanır və video tam
                    -- hazır olur; əvvəlki sıralamada bütün 1080p-lər əvvəl
                    -- götürüldüyü üçün heç bir video tamamlanmırdı.
                    ORDER BY
                        CASE WHEN {preferVideoId}::bigint IS NOT NULL AND video_id = {preferVideoId} THEN 0 ELSE 1 END,
                        video_id,
                        priority DESC,
                        id
                    LIMIT {take}
                    FOR UPDATE SKIP LOCKED
                    """)
                .IgnoreQueryFilters()
                .ToListAsync(ct);

            if (jobs.Count == 0)
            {
                await tx.CommitAsync(ct);
                return [];
            }

            foreach (var job in jobs)
            {
                job.Status = JobStatus.Assigned;
                job.WorkerNodeId = node.Id;
                job.AttemptToken = Guid.NewGuid().ToString("N");
                job.Attempts += 1;
                job.LeaseExpiresAt = lease;
                job.NextAttemptAt = null;
                job.StartedAt ??= DateTimeOffset.UtcNow;
                job.Progress = 0;
                job.ErrorMessage = null;
                job.ErrorKind = null;
                job.TenantId = node.TenantId;
            }

            node.ActiveJobs += jobs.Count;
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        return await BuildAssignmentsAsync(jobs, ct);
    }

    private async Task<IReadOnlyList<JobAssignment>> BuildAssignmentsAsync(List<VideoJob> jobs, CancellationToken ct)
    {
        var videoIds = jobs.Select(j => j.VideoId).Distinct().ToList();
        var prefixes = jobs.Select(j => j.OutputPrefix).Distinct().ToList();

        var videos = await db.Videos.IgnoreQueryFilters()
            .Where(v => videoIds.Contains(v.Id) && v.DeletedAt == null)
            .Select(v => new { v.Id, v.Path, v.DurationSeconds })
            .ToListAsync(ct);

        // HEAD sorğusu ilə ölçü öyrənilir: böyük videonu tanımaq üçün lazımdır.
        var sizes = new Dictionary<long, long>();

        foreach (var video in videos)
        {
            if (await storage.GetSizeAsync(video.Path, ct) is { } size) sizes[video.Id] = size;
        }

        // Merdiven bu versiyanın qardaş tapşırıqlarından qurulur — koordinator master
        // playlist-i yalnız həqiqətən mövcud olan variantlardan yazır.
        var siblings = await db.VideoJobs.IgnoreQueryFilters()
            .Where(j => prefixes.Contains(j.OutputPrefix) && !j.Skipped && j.Status != JobStatus.Cancelled)
            .Select(j => new { j.OutputPrefix, j.Resolution, j.TargetHeight, j.TargetBitrateKbps })
            .ToListAsync(ct);

        var expiry = TimeSpan.FromMinutes(Settings.DownloadUrlExpiryMinutes);
        var result = new List<JobAssignment>(jobs.Count);

        foreach (var job in jobs)
        {
            var video = videos.First(v => v.Id == job.VideoId);
            var sourceUrl = storage.CreatePresignedGetUrl(video.Path, expiry) ?? storage.GetUrl(video.Path);

            var ladder = siblings
                .Where(s => s.OutputPrefix == job.OutputPrefix)
                .Select(s => new EncodingProfile
                {
                    Name = s.Resolution,
                    Height = s.TargetHeight,
                    VideoBitrateKbps = s.TargetBitrateKbps
                })
                .OrderBy(p => p.Height)
                .ToList();

            result.Add(new JobAssignment
            {
                JobId = job.Id,
                VideoId = job.VideoId,
                Resolution = job.Resolution,
                AttemptToken = job.AttemptToken!,
                Attempt = job.Attempts,
                SourceUrl = sourceUrl,
                SourceKey = video.Path,
                OutputPrefix = job.OutputPrefix,
                MasterKey = NodePaths.MasterKey(job.OutputPrefix),
                DurationSeconds = video.DurationSeconds,
                SourceBytes = sizes.TryGetValue(video.Id, out var size) ? size : null,
                SourceHeight = job.SourceHeight,
                SourceWidth = job.SourceWidth,
                SourceFps = job.SourceFps,
                HasAudio = job.HasAudio,
                Profile = new EncodingProfile
                {
                    Name = job.Resolution,
                    Height = job.TargetHeight,
                    VideoBitrateKbps = job.TargetBitrateKbps
                },
                Ladder = ladder.Count > 0 ? ladder : [new EncodingProfile
                {
                    Name = job.Resolution,
                    Height = job.TargetHeight,
                    VideoBitrateKbps = job.TargetBitrateKbps
                }],
                LeaseExpiresAt = job.LeaseExpiresAt ?? DateTimeOffset.UtcNow.AddMinutes(Settings.LeaseMinutes)
            });
        }
        return result;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: "23505" };
}
