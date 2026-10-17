using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Media.Entities;
using Kela.Core.Modules.Nodes;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Core.Modules.Nodes.Requests;
using Kela.Nodes.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Tests.Nodes;

using VideoJob = Kela.Core.Modules.Nodes.Entities.VideoJob;
using WorkerNode = Kela.Core.Modules.Nodes.Entities.WorkerNode;

/// <summary>Nodes testləri üçün ortaq qurğu: video, cihaz və iş axını köməkçiləri.</summary>
internal sealed class NodeTestHarness(ApiFactory factory)
{
    private ApiFactory Factory { get; } = factory;

    public static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    public async Task<TResult> ServiceAsync<TService, TResult>(Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        using var scope = Factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    public async Task<TResult> DbAsync<TResult>(Func<AppDbContext, Task<TResult>> action)
    {
        using var scope = Factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public async Task DbAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    /// <summary>Dərs + video kontenti yaradır; tapşırıqlar avtomatik yaranır.</summary>
    public async Task<(long VideoId, string Path)> CreateVideoAsync(int? durationSeconds = null)
    {
        var subjectId = await TestData.SubjectAsync(Factory, Unique("Subject"));
        var lessonId = await TestData.LessonAsync(Factory, subjectId, Unique("Lesson"));
        var contentId = await TestData.LessonContentAsync(Factory, lessonId, Unique("Video"));

        var videoId = await DbAsync(async db =>
        {
            var content = await db.LessonContents.Include(x => x.Video).FirstAsync(x => x.Id == contentId);
            content.Video!.DurationSeconds = durationSeconds;
            await db.SaveChangesAsync();
            return content.Video.Id;
        });

        return (videoId, await VideoPathAsync(videoId));
    }

    /// <summary>
    /// Eyni mənbə faylına işarə edən ikinci video yaradır — bir fayl bir neçə dərsdə
    /// istifadə oluna bilər və o halda yol üzrə axtarış qeyri-müəyyən olur.
    /// </summary>
    public async Task<long> CreateSecondVideoForSameSourceAsync(string path)
    {
        var subjectId = await TestData.SubjectAsync(Factory, Unique("Subject"));
        var lessonId = await TestData.LessonAsync(Factory, subjectId, Unique("Lesson"));

        var content = await ServiceAsync<Core.Modules.LessonContent.Abstractions.ILessonContentService,
            Core.Modules.LessonContent.Responses.LessonContentResponse>(
            s => s.CreateAsync(new Core.Modules.LessonContent.Requests.LessonContentCreateRequest
            {
                LessonId = lessonId,
                Title = Unique("Video"),
                Type = Core.Modules.LessonContent.Entities.LessonContentType.Video,
                IsActive = true,
                Path = path
            }));

        return await DbAsync(db => db.LessonContents.Where(c => c.Id == content.Id)
            .Select(c => c.Video!.Id).FirstAsync());
    }

    public Task<string> VideoPathAsync(long videoId)
        => DbAsync(async db => await db.Videos.IgnoreQueryFilters()
            .Where(v => v.Id == videoId).Select(v => v.Path).FirstAsync());

    public Task<List<VideoJob>> JobsAsync(long videoId)
        => DbAsync(async db => await db.VideoJobs.IgnoreQueryFilters()
            .Where(j => j.VideoId == videoId).OrderBy(j => j.TargetHeight).ToListAsync());

    public async Task<WorkerNode> RegisterNodeAsync(string name, int maxConcurrentJobs = 1)
    {
        var code = await ServiceAsync<INodeService, string>(async s =>
            (await s.CreateCodeAsync(new CreateRegistrationCodeRequest { Note = name })).Code);

        var response = await ServiceAsync<INodeService, NodeRegisterResponse>(s => s.RegisterAsync(new NodeRegisterRequest
        {
            RegistrationCode = code,
            Name = name,
            Device = new NodeDeviceInfo { OperatingSystem = "Linux", CpuCores = 4, Encoders = ["libx264"] }
        }));

        return await DbAsync(async db =>
        {
            var node = await db.WorkerNodes.IgnoreQueryFilters().FirstAsync(n => n.Id == response.NodeId);
            node.MaxConcurrentJobs = maxConcurrentJobs;
            await db.SaveChangesAsync();
            return node;
        });
    }

    public Task<IReadOnlyList<JobAssignment>> ClaimAsync(WorkerNode node, int max = 1)
        => ServiceAsync<IVideoJobScheduler, IReadOnlyList<JobAssignment>>(s => s.ClaimAsync(node, max));

    /// <summary>
    /// Test bazası bütün testlərin videolarını saxlayır, ona görə claim nəticəsi
    /// verilmiş video üzrə süzülür.
    /// </summary>
    public async Task<List<JobAssignment>> ClaimForVideoAsync(WorkerNode node, long videoId, int max = 64)
    {
        var mine = new List<JobAssignment>();

        for (var pass = 0; pass < 8; pass++)
        {
            var claimed = await ClaimAsync(node, max);
            if (claimed.Count == 0) break;

            foreach (var assignment in claimed.Where(a => a.VideoId != videoId))
            {
                // Başqa testin videosu — dərhal geri qaytarılır ki, yer boşalsın.
                await ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.FailAsync(node, assignment.JobId,
                    new JobFailRequest { AttemptToken = assignment.AttemptToken, Error = "not ours", Kind = JobErrorKind.Transient }));
            }

            mine.AddRange(claimed.Where(a => a.VideoId == videoId));

            if (claimed.All(a => a.VideoId == videoId)) break;
        }

        return mine;
    }

    /// <summary>Reaper-dən sonra backoff pəncərəsini geri sarır — vaxtın keçməsini təqlid edir.</summary>
    public Task ExpireBackoffAsync()
        => DbAsync(async db =>
        {
            foreach (var job in await db.VideoJobs.IgnoreQueryFilters()
                         .Where(j => j.NextAttemptAt != null).ToListAsync())
            {
                job.NextAttemptAt = null;
            }

            await db.SaveChangesAsync();
        });

    public Task<JobOperationResult> ReapAsync()
        => ServiceAsync<IVideoJobScheduler, JobOperationResult>(async s =>
        {
            await s.ReapAsync();
            return JobOperationResult.Ok();
        });

    public async Task<JobOperationResult> ReconcileAndFinalizeAsync(long videoId)
    {
        var pending = await ServiceAsync<IVideoJobScheduler, IReadOnlyList<long>>(s => s.ReconcileAsync());
        if (!pending.Contains(videoId)) return JobOperationResult.Ok();

        return await ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.FinalizeAsync(videoId));
    }

    public Task PutAsync(string key, string content)
        => ServiceAsync<IFileStorage, bool>(async s =>
        {
            await s.UploadTextAsync(key, content, "application/octet-stream");
            return true;
        });

    public Task<string?> ReadAsync(string key)
        => ServiceAsync<IFileStorage, string?>(s => s.ReadTextAsync(key));

    public Task<bool> ExistsAsync(string key)
        => ServiceAsync<IFileStorage, bool>(s => s.ExistsAsync(key));

    /// <summary>Tam bir HLS variantı (playlist + 2 seqment) yazır və tamamlama sorğusunu göndərir.</summary>
    public async Task<JobOperationResult> CompleteVariantAsync(WorkerNode node, JobAssignment assignment)
    {
        var res = assignment.Resolution;
        var folder = $"{assignment.OutputPrefix}/{res}";

        await PutAsync($"{folder}/segment_00001.ts", "segment-one");
        await PutAsync($"{folder}/segment_00002.ts", "segment-two");
        await PutAsync($"{folder}/index.m3u8",
            "#EXTM3U\n#EXT-X-VERSION:3\n#EXT-X-TARGETDURATION:8\n#EXTINF:8.0,\nsegment_00001.ts\n#EXTINF:8.0,\nsegment_00002.ts\n#EXT-X-ENDLIST\n");
        await PutAsync($"{assignment.OutputPrefix}/thumb.jpg", "thumbnail");

        return await ServiceAsync<IVideoJobLifecycle, JobOperationResult>(s => s.CompleteAsync(node, assignment.JobId,
            new JobCompleteRequest
            {
                AttemptToken = assignment.AttemptToken,
                DurationSeconds = 380,
                ThumbnailKey = "thumb.jpg",
                VariantKey = $"{res}/index.m3u8",
                Objects =
                [
                    $"{res}/index.m3u8", $"{res}/segment_00001.ts", $"{res}/segment_00002.ts", "thumb.jpg"
                ],
                Fps = 30,
                Encoder = "libx264"
            }));
    }

    /// <summary>Worker bir işi götürüb tamamlayır.</summary>
    public async Task<JobOperationResult> RunOneJobAsync(WorkerNode node)
    {
        var assignment = (await ClaimAsync(node)).FirstOrDefault();
        if (assignment is null) return JobOperationResult.NotFound();

        return await CompleteVariantAsync(node, assignment);
    }

    public Task<Video?> VideoAsync(long videoId)
        => DbAsync(db => db.Videos.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.Id == videoId));

    public Task<VideoJob?> JobAsync(long jobId)
        => DbAsync(db => db.VideoJobs.IgnoreQueryFilters().FirstOrDefaultAsync(j => j.Id == jobId));
}
