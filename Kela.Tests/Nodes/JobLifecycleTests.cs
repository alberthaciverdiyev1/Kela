using Kela.Core.Modules.Media.Entities;
using Kela.Core.Modules.Nodes;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Nodes.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kela.Tests.Nodes;

[Collection("api")]
public class JobLifecycleTests(ApiFactory factory)
{
    private readonly NodeTestHarness _h = new(factory);

    [Fact]
    public async Task Completing_every_ladder_step_publishes_the_master_playlist()
    {
        var (videoId, _) = await _h.CreateVideoAsync(durationSeconds: 380);
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        foreach (var assignment in await _h.ClaimForVideoAsync(node, videoId))
        {
            Assert.True((await _h.CompleteVariantAsync(node, assignment)).IsOk);
        }

        var video = await _h.VideoAsync(videoId);

        Assert.Equal(VideoStatus.Ready, video!.ConversionStatus);
        Assert.Null(video.ConversionError);
        Assert.Equal(380, video.DurationSeconds);
        Assert.NotNull(video.Thumbnail);

        var master = await _h.ReadAsync(video.PlaybackUrl!);

        Assert.NotNull(master);
        Assert.StartsWith("#EXTM3U", master);
        Assert.Contains("BANDWIDTH=", master);

        foreach (var resolution in new[] { "360p", "480p", "720p", "1080p" })
        {
            Assert.Contains($"{resolution}/index.m3u8", master);
            Assert.True(await _h.ExistsAsync($"{video.PlaybackUrl![..video.PlaybackUrl!.LastIndexOf('/')]}/{resolution}/index.m3u8"));
        }
    }

    [Fact]
    public async Task The_same_job_cannot_be_completed_twice()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        Assert.True((await _h.CompleteVariantAsync(node, assignment)).IsOk);

        var second = await _h.CompleteVariantAsync(node, assignment);

        Assert.Equal(JobOperationStatus.Conflict, second.Status);
        Assert.True((await _h.JobAsync(assignment.JobId))!.AttemptToken is null);

        // İkinci cəhd nəticəni dəyişmədi.
        Assert.Equal(JobStatus.Completed, (await _h.JobAsync(assignment.JobId))!.Status);
    }

    [Fact]
    public async Task A_stale_attempt_cannot_overwrite_a_newer_one()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var first = (await _h.ClaimForVideoAsync(node, videoId)).First();

        // Lease bitir → iş yenidən kuyruğa düşür və başqa cəhd tokeni alır.
        await _h.DbAsync(async db =>
        {
            var job = await db.VideoJobs.IgnoreQueryFilters().FirstAsync(j => j.Id == first.JobId);
            job.LeaseExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5);
            await db.SaveChangesAsync();
        });

        await _h.ReapAsync();
        await _h.ExpireBackoffAsync();

        var retried = (await _h.ClaimForVideoAsync(node, videoId)).FirstOrDefault(a => a.JobId == first.JobId);
        Assert.NotNull(retried);
        Assert.NotEqual(first.AttemptToken, retried!.AttemptToken);

        var stale = await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.CompleteAsync(node, first.JobId,
            new JobCompleteRequest { AttemptToken = first.AttemptToken, VariantKey = "360p/index.m3u8" }));

        Assert.Equal(JobOperationStatus.Conflict, stale.Status);
    }

    [Fact]
    public async Task Validation_rejects_a_variant_whose_segments_are_missing()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        var folder = $"{assignment.OutputPrefix}/{assignment.Resolution}";
        await _h.PutAsync($"{folder}/segment_00001.ts", "one");
        await _h.PutAsync($"{folder}/index.m3u8",
            "#EXTM3U\n#EXTINF:8.0,\nsegment_00001.ts\n#EXTINF:8.0,\nsegment_00002.ts\n#EXT-X-ENDLIST\n");

        var result = await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.CompleteAsync(node, assignment.JobId,
            new JobCompleteRequest { AttemptToken = assignment.AttemptToken, VariantKey = $"{assignment.Resolution}/index.m3u8" }));

        Assert.Equal(JobOperationStatus.Invalid, result.Status);
        Assert.Contains("segment_00002.ts", result.Error);

        // İş itmir — worker düzəldib yenidən göndərə bilər.
        Assert.Equal(JobStatus.Processing, (await _h.JobAsync(assignment.JobId))!.Status);
    }

    [Fact]
    public async Task Upload_keys_outside_the_job_folder_are_rejected()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        foreach (var key in new[] { "../../other/index.m3u8", "/etc/passwd", "999p/index.m3u8", "http://evil/x.ts" })
        {
            var (result, urls) = await _h.ServiceAsync<IVideoJobLifecycle, (JobOperationResult, JobUploadUrlResponse?)>(
                s => s.CreateUploadUrlsAsync(node, assignment.JobId,
                    new JobUploadUrlRequest { AttemptToken = assignment.AttemptToken, Keys = [key] }));

            Assert.Equal(JobOperationStatus.Invalid, result.Status);
            Assert.Null(urls);
        }
    }

    [Fact]
    public async Task A_foreign_node_cannot_touch_someone_elses_job()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var owner = await _h.RegisterNodeAsync(NodeTestHarness.Unique("owner"), maxConcurrentJobs: 64);
        var intruder = await _h.RegisterNodeAsync(NodeTestHarness.Unique("intruder"), maxConcurrentJobs: 64);

        var assignment = (await _h.ClaimForVideoAsync(owner, videoId)).First();

        var result = await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.CompleteAsync(intruder, assignment.JobId,
            new JobCompleteRequest { AttemptToken = assignment.AttemptToken, VariantKey = $"{assignment.Resolution}/index.m3u8" }));

        Assert.Equal(JobOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task A_transient_failure_requeues_the_job_with_backoff()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.FailAsync(node, assignment.JobId,
            new JobFailRequest { AttemptToken = assignment.AttemptToken, Error = "network hiccup", Kind = JobErrorKind.Transient }));

        var job = await _h.JobAsync(assignment.JobId);

        Assert.Equal(JobStatus.Pending, job!.Status);
        Assert.NotNull(job.NextAttemptAt);
        Assert.True(job.NextAttemptAt > DateTimeOffset.UtcNow);
        Assert.Null(job.WorkerNodeId);
    }

    [Fact]
    public async Task An_expired_lease_returns_the_job_to_the_queue()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        await _h.DbAsync(async db =>
        {
            var job = await db.VideoJobs.IgnoreQueryFilters().FirstAsync(j => j.Id == assignment.JobId);
            job.Status = JobStatus.Processing;
            job.LeaseExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
            await db.SaveChangesAsync();
        });

        await _h.ReapAsync();

        var job = await _h.JobAsync(assignment.JobId);

        Assert.Equal(JobStatus.Pending, job!.Status);
        Assert.Null(job.WorkerNodeId);
        Assert.Null(job.AttemptToken);
        Assert.Null(job.LeaseExpiresAt);
        Assert.Equal(1, job.Attempts);
    }

    [Fact]
    public async Task A_permanent_failure_fails_the_job_and_the_video()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var claimed = await _h.ClaimForVideoAsync(node, videoId);

        foreach (var assignment in claimed)
        {
            await _h.ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.FailAsync(node, assignment.JobId,
                new JobFailRequest { AttemptToken = assignment.AttemptToken, Error = "corrupt source", Kind = JobErrorKind.Permanent }));
        }

        Assert.All(await _h.JobsAsync(videoId), j => Assert.Equal(JobStatus.Failed, j.Status));

        var video = await _h.VideoAsync(videoId);
        Assert.Equal(VideoStatus.Failed, video!.ConversionStatus);
        Assert.Contains("corrupt source", video.ConversionError);
        Assert.Null(video.PlaybackUrl);
    }

    [Fact]
    public async Task The_video_is_not_published_while_any_ladder_step_is_outstanding()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);

        var claimed = await _h.ClaimForVideoAsync(node, videoId);

        // Yalnız bir pillə bitir — master playlist yazılmamalıdır.
        await _h.CompleteVariantAsync(node, claimed.First(a => a.Resolution == "1080p"));

        var video = await _h.VideoAsync(videoId);

        Assert.NotEqual(VideoStatus.Ready, video!.ConversionStatus);
        Assert.Null(video.PlaybackUrl);
    }

    [Fact]
    public async Task A_worker_reporting_after_reconnect_is_not_duplicated()
    {
        var (videoId, _) = await _h.CreateVideoAsync();
        var node = await _h.RegisterNodeAsync(NodeTestHarness.Unique("node"), maxConcurrentJobs: 64);
        var assignment = (await _h.ClaimForVideoAsync(node, videoId)).First();

        // Cihaz qoşulmasını itirir, lease bitir, iş yenidən kuyruğa düşür.
        await _h.DbAsync(async db =>
        {
            var job = await db.VideoJobs.IgnoreQueryFilters().FirstAsync(j => j.Id == assignment.JobId);
            job.LeaseExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        });

        await _h.ReapAsync();
        await _h.ExpireBackoffAsync();

        // Eyni cihaz yenidən qoşulur və işi təzədən götürür — ikinci nüsxə yaranmır.
        var retried = (await _h.ClaimForVideoAsync(node, videoId)).First(a => a.JobId == assignment.JobId);

        var jobs = await _h.JobsAsync(videoId);

        Assert.Equal(4, jobs.Count);
        Assert.Equal(2, jobs.Single(j => j.Id == assignment.JobId).Attempts);
        Assert.Equal(JobStatus.Assigned, (await _h.JobAsync(retried.JobId))!.Status);
    }
}
