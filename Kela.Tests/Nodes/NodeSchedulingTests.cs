using Kela.Core.Data;
using Kela.Core.Modules.Media.Entities;
using Kela.Core.Modules.Nodes;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Nodes.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kela.Tests.Nodes;

[Collection("api")]
public class NodeSchedulingTests(ApiFactory factory)
{
    private readonly NodeTestHarness _h = new(factory);

    [Fact]
    public async Task Uploading_a_video_creates_one_job_per_ladder_step()
    {
        var (videoId, _) = await _h.CreateVideoAsync();

        var jobs = await _h.JobsAsync(videoId);

        Assert.Equal(4, jobs.Count);
        Assert.Equal(["360p", "480p", "720p", "1080p"], jobs.Select(j => j.Resolution));
        Assert.All(jobs, j => Assert.Equal(JobStatus.Pending, j.Status));
    }

    [Fact]
    public async Task Ensuring_jobs_twice_does_not_duplicate_them()
    {
        var (videoId, _) = await _h.CreateVideoAsync();

        await _h.ServiceAsync<IVideoJobScheduler, bool>(async s =>
        {
            await s.EnsureJobsAsync(videoId);
            await s.EnsureJobsAsync(videoId);
            return true;
        });

        Assert.Equal(4, (await _h.JobsAsync(videoId)).Count);
    }

    [Fact]
    public async Task Jobs_of_the_same_video_carry_the_same_output_prefix()
    {
        var (videoId, path) = await _h.CreateVideoAsync();
        var jobs = await _h.JobsAsync(videoId);

        var prefix = jobs.Select(j => j.OutputPrefix).Distinct().Single();

        // Çıxış mənbə faylının tenant qovluğu altında, oxunaqlı adla və versiyalı olmalıdır.
        Assert.StartsWith(path[..path.IndexOf("/videos/raw/", StringComparison.Ordinal)], prefix);
        Assert.EndsWith($"/v1", prefix);

        var folder = prefix.Split('/')[^2];
        Assert.StartsWith($"{videoId}-", folder);
        Assert.DoesNotContain(" ", folder);
        Assert.Equal(folder.ToLowerInvariant(), folder);
    }

    [Fact]
    public async Task Every_ladder_step_of_a_video_lands_on_the_queue()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        var claimed = await _h.ClaimForVideoAsync(node, videoId);

        Assert.Equal(4, claimed.Count);
        Assert.All(claimed, a => Assert.NotEmpty(a.AttemptToken));
        Assert.All(claimed, a => Assert.True(a.LeaseExpiresAt > DateTimeOffset.UtcNow));
        Assert.Equal(4, (await _h.JobsAsync(videoId)).Count(j => j.Status == JobStatus.Assigned));
    }

    [Fact]
    public async Task A_node_never_receives_more_jobs_than_its_capacity()
    {
        await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 2);

        var claimed = await _h.ClaimAsync(node, 64);

        Assert.True(claimed.Count <= 2, $"capacity was 2 but {claimed.Count} jobs were handed out");

        var active = await _h.DbAsync(db => db.VideoJobs.IgnoreQueryFilters()
            .CountAsync(j => j.WorkerNodeId == node.Id && j.Status == JobStatus.Assigned));

        Assert.True(active <= 2);
    }

    [Fact]
    public async Task A_job_is_never_handed_to_two_nodes()
    {
        await _h.CreateVideoAsync();
        var first = await _h.RegisterNodeAsync(NodeTestHarness.Unique("first"), maxConcurrentJobs: 64);
        var second = await _h.RegisterNodeAsync(NodeTestHarness.Unique("second"), maxConcurrentJobs: 64);

        var a = await _h.ClaimAsync(first, 64);
        var b = await _h.ClaimAsync(second, 64);

        Assert.NotEmpty(a);
        Assert.Empty(a.Select(x => x.JobId).Intersect(b.Select(x => x.JobId)));
    }

    [Fact]
    public async Task Source_media_cancels_ladder_steps_above_the_source_height()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First(a => a.Resolution == "360p");

        await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.ReportProgressAsync(node, assignment.JobId,
            new JobProgressRequest
            {
                AttemptToken = assignment.AttemptToken,
                Status = JobStatus.Processing,
                Progress = 0.1,
                Media = new SourceMediaInfo { Width = 854, Height = 480, Fps = 25, DurationSeconds = 380, HasAudio = true }
            }));

        var jobs = await _h.JobsAsync(videoId);

        Assert.All(jobs.Where(j => j.TargetHeight > 480), j =>
        {
            Assert.Equal(JobStatus.Cancelled, j.Status);
            Assert.True(j.Skipped);
        });

        Assert.DoesNotContain(jobs.Where(j => j.TargetHeight <= 480), j => j.Skipped);
        Assert.All(jobs, j => Assert.Equal(480, j.SourceHeight));
    }

    [Fact]
    public async Task Publishing_after_a_smaller_source_skips_the_upscaled_variants()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        var claimed = await _h.ClaimForVideoAsync(node, videoId);
        var low = claimed.First(a => a.Resolution == "360p");

        await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.ReportProgressAsync(node, low.JobId,
            new JobProgressRequest
            {
                AttemptToken = low.AttemptToken,
                Status = JobStatus.Processing,
                Media = new SourceMediaInfo { Width = 854, Height = 480, Fps = 25, DurationSeconds = 380 }
            }));

        // Mənbədən böyük pillələr ləğv olundu — yalnız 360p və 480p kodlaşdırılır.
        foreach (var assignment in claimed.Where(a => a.Resolution is "360p" or "480p"))
        {
            Assert.True((await _h.CompleteVariantAsync(node, assignment)).IsOk);
        }

        await _h.ReconcileAndFinalizeAsync(videoId);

        var video = await _h.VideoAsync(videoId);
        Assert.Equal(VideoStatus.Ready, video!.ConversionStatus);

        var master = await _h.ReadAsync(video.PlaybackUrl!);
        Assert.NotNull(master);
        Assert.Contains("360p/index.m3u8", master);
        Assert.Contains("480p/index.m3u8", master);
        Assert.DoesNotContain("720p/index.m3u8", master);
        Assert.DoesNotContain("1080p/index.m3u8", master);
    }
}
