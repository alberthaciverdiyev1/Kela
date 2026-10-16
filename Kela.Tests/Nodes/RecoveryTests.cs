using Kela.Core.Modules.Media.Entities;
using Kela.Core.Modules.Nodes;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Nodes.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kela.Tests.Nodes;

/// <summary>
/// Xəta halları: endirmə/yükləmə nasazlığı, cihazın sönməsi, koordinatorun çökməsi.
/// Video heç bir halda "bozulmuş" vəziyyətdə ilişib qalmamalıdır.
/// </summary>
[Collection("api")]
public class RecoveryTests(ApiFactory factory)
{
    private readonly NodeTestHarness _h = new(factory);

    [Fact]
    public async Task A_job_stuck_in_validating_is_reclaimed_after_its_lease()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        // Koordinator doğrulama zamanı çökdü: tapşırıq "validating" vəziyyətində qaldı.
        await _h.DbAsync(async db =>
        {
            var job = await db.VideoJobs.IgnoreQueryFilters().FirstAsync(j => j.Id == assignment.JobId);
            job.Status = JobStatus.Validating;
            job.LeaseExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        });

        await _h.ReapAsync();

        var job = await _h.JobAsync(assignment.JobId);

        Assert.Equal(JobStatus.Pending, job!.Status);
        Assert.Null(job.WorkerNodeId);
        Assert.Null(job.LeaseExpiresAt);
    }

    [Fact]
    public async Task A_dead_devices_jobs_are_released_without_waiting_for_the_full_lease()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        // Cihaz söndü: heartbeat yoxdur, lease isə hələ bitməyib.
        await _h.DbAsync(async db =>
        {
            var job = await db.VideoJobs.IgnoreQueryFilters().FirstAsync(j => j.Id == assignment.JobId);
            job.Status = JobStatus.Processing;
            job.LeaseExpiresAt = DateTimeOffset.UtcNow.AddMinutes(25);
            await db.SaveChangesAsync();

            var worker = await db.WorkerNodes.IgnoreQueryFilters().FirstAsync(n => n.Id == node.Id);
            worker.Status = NodeStatus.Offline;
            worker.LastSeenAt = DateTimeOffset.UtcNow.AddMinutes(-10);
            await db.SaveChangesAsync();
        });

        await _h.ReapAsync();

        var job = await _h.JobAsync(assignment.JobId);

        // 25 dəqiqəlik lease gözlənilmədi — cihaz offline olduğu üçün dərhal buraxıldı.
        Assert.Equal(JobStatus.Pending, job!.Status);
        Assert.Null(job.WorkerNodeId);
    }

    [Fact]
    public async Task A_healthy_devices_long_lease_is_not_taken_away()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        await _h.DbAsync(async db =>
        {
            var job = await db.VideoJobs.IgnoreQueryFilters().FirstAsync(j => j.Id == assignment.JobId);
            job.Status = JobStatus.Processing;
            job.LeaseExpiresAt = DateTimeOffset.UtcNow.AddMinutes(25);
            await db.SaveChangesAsync();

            var worker = await db.WorkerNodes.IgnoreQueryFilters().FirstAsync(n => n.Id == node.Id);
            worker.Status = NodeStatus.Online;
            worker.LastSeenAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        });

        await _h.ReapAsync();

        Assert.Equal(JobStatus.Processing, (await _h.JobAsync(assignment.JobId))!.Status);
    }

    [Fact]
    public async Task A_job_that_hangs_forever_is_taken_back()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        // Cihaz canlıdır və heartbeat göndərir, amma iş saatlardır bitmir.
        // Lease hər dəfə uzadıldığı üçün əvvəllər bu tapşırıq ələ keçmirdi.
        await _h.DbAsync(async db =>
        {
            var job = await db.VideoJobs.IgnoreQueryFilters().FirstAsync(j => j.Id == assignment.JobId);
            job.Status = JobStatus.Processing;
            job.LeaseExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
            job.StartedAt = DateTimeOffset.UtcNow.AddHours(-10);
            await db.SaveChangesAsync();

            var worker = await db.WorkerNodes.IgnoreQueryFilters().FirstAsync(n => n.Id == node.Id);
            worker.Status = NodeStatus.Online;
            worker.LastSeenAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        });

        await _h.ReapAsync();

        Assert.Equal(JobStatus.Pending, (await _h.JobAsync(assignment.JobId))!.Status);
    }

    [Fact]
    public async Task A_network_failure_retries_the_whole_video_instead_of_marking_it_broken()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        // Bütün pillələr keçici xəta ilə uğursuz olur (məsələn şəbəkə kəsildi).
        foreach (var assignment in await _h.ClaimForVideoAsync(node, videoId))
        {
            await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.FailAsync(node, assignment.JobId,
                new JobFailRequest
                {
                    AttemptToken = assignment.AttemptToken,
                    Error = "An error occurred while sending the request.",
                    Kind = JobErrorKind.Transient
                }));
        }

        var video = await _h.VideoAsync(videoId);

        // Video "bozulmuş" sayılmır — tapşırıqlar geri çevrilib.
        Assert.NotEqual(VideoStatus.Failed, video!.ConversionStatus);

        var jobs = await _h.JobsAsync(videoId);

        Assert.All(jobs, job => Assert.Equal(JobStatus.Pending, job.Status));
        Assert.All(jobs, job => Assert.NotNull(job.NextAttemptAt));
    }

    [Fact]
    public async Task A_retried_video_can_still_be_published()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        var claimed = await _h.ClaimForVideoAsync(node, videoId);

        // Birinci dövr uğursuz olur.
        foreach (var assignment in claimed)
        {
            await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.FailAsync(node, assignment.JobId,
                new JobFailRequest { AttemptToken = assignment.AttemptToken, Error = "şəbəkə", Kind = JobErrorKind.Transient }));
        }

        await _h.ExpireBackoffAsync();

        // İkinci dövr uğurla tamamlanır.
        foreach (var assignment in await _h.ClaimForVideoAsync(node, videoId))
        {
            await _h.CompleteVariantAsync(node, assignment);
        }

        var video = await _h.VideoAsync(videoId);

        Assert.Equal(VideoStatus.Ready, video!.ConversionStatus);
        Assert.NotNull(video.PlaybackUrl);
    }

    [Fact]
    public async Task A_permanent_failure_still_marks_the_video_broken()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        foreach (var assignment in await _h.ClaimForVideoAsync(node, videoId))
        {
            await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.FailAsync(node, assignment.JobId,
                new JobFailRequest
                {
                    AttemptToken = assignment.AttemptToken,
                    Error = "moov atom not found",
                    Kind = JobErrorKind.Permanent
                }));
        }

        var video = await _h.VideoAsync(videoId);

        // Qalıcı xəta təkrar sınanmır.
        Assert.Equal(VideoStatus.Failed, video!.ConversionStatus);
        Assert.Equal(0, video.ConversionAttempts);
    }

    [Fact]
    public async Task The_retry_budget_is_finite()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        // Hər dövr tapşırıq cəhdini bir artırır; büdcə = max_attempts × max_video_attempts.
        for (var round = 0; round < 40; round++)
        {
            if ((await _h.VideoAsync(videoId))?.ConversionStatus == VideoStatus.Failed) break;

            await _h.ExpireBackoffAsync();

            var claimed = await _h.ClaimForVideoAsync(node, videoId);
            if (claimed.Count == 0) break;

            foreach (var assignment in claimed)
            {
                await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.FailAsync(node, assignment.JobId,
                    new JobFailRequest { AttemptToken = assignment.AttemptToken, Error = "şəbəkə", Kind = JobErrorKind.Transient }));
            }
        }

        // Büdcə bitdikdən sonra video artıq "failed" olur — sonsuz təkrar yoxdur.
        Assert.Equal(VideoStatus.Failed, (await _h.VideoAsync(videoId))!.ConversionStatus);
    }
}
