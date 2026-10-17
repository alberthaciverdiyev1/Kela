using Kela.Core.Modules.Media.Entities;
using Kela.Core.Modules.Nodes.Requests;
using Kela.Nodes.Shared.Contracts;
using Kela.Nodes.Worker;
using Kela.Nodes.Worker.Encoding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Tests.Nodes;

/// <summary>
/// Ayrı verilənlər bazası — worker bütün gözləyən işləri götürdüyü üçün digər
/// testlərin videoları buna mane olmamalıdır.
/// </summary>
[CollectionDefinition("worker-e2e")]
public sealed class WorkerE2ECollection : ICollectionFixture<ApiFactory>;

/// <summary>
/// Ən dəyərli test: həqiqi ffmpeg ilə işləyən worker həqiqi API-yə qoşulur, videonu
/// endirir, HLS-ə çevirir, yükləyir və Edutor qeydi "ready" olur.
/// </summary>
[Collection("worker-e2e")]
public class WorkerEndToEndTests(ApiFactory factory) : IDisposable
{
    private readonly NodeTestHarness _h = new(factory);
    private readonly string _cache = Path.Combine(Path.GetTempPath(), $"kela-worker-e2e-{Guid.NewGuid():N}");

    private static string? Executable(string name)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

        return path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => Path.Combine(directory.Trim(), name))
            .FirstOrDefault(File.Exists);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_cache)) Directory.Delete(_cache, recursive: true);
        }
        catch (IOException)
        {
            // Keş qovluğu qala bilər.
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>Storage-a real (ffmpeg ilə yaradılmış) video qoyur və Edutor qeydini yaradır.</summary>
    private async Task<long> SeedRealVideoAsync(int seconds = 6)
    {
        var ffmpeg = Executable("ffmpeg");
        Assert.NotNull(ffmpeg);

        var relativeKey = $"shared/videos/raw/e2e-{Guid.NewGuid():N}.mp4";
        var fullPath = Path.Combine(factory.StorageRoot, relativeKey);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var produced = await ProcessRunner.RunAsync(ffmpeg!, [
            "-hide_banner", "-nostdin", "-loglevel", "error", "-y",
            "-f", "lavfi", "-i", $"testsrc2=size=1280x720:rate=25:duration={seconds}",
            "-f", "lavfi", "-i", $"sine=frequency=440:duration={seconds}",
            "-shortest", "-c:v", "libx264", "-preset", "ultrafast", "-c:a", "aac", fullPath
        ], timeout: TimeSpan.FromMinutes(3));

        Assert.True(produced.Success, produced.Error);
        Assert.True(File.Exists(fullPath), $"ffmpeg faylı yaratmadı: {fullPath}");

        var provider = await _h.ServiceAsync<Core.Common.Storage.IFileStorage, string>(s => Task.FromResult(s.GetType().Name));
        var storageSees = await _h.ServiceAsync<Core.Common.Storage.IFileStorage, bool>(s => s.ExistsAsync(relativeKey));

        Assert.True(storageSees,
            $"Storage '{relativeKey}' faylını görmür. Sağlayıcı: {provider}, root: {factory.StorageRoot}, fayl var: {File.Exists(fullPath)}");

        var subjectId = await TestData.SubjectAsync(factory, NodeTestHarness.Unique("Subject"));
        var lessonId = await TestData.LessonAsync(factory, subjectId, NodeTestHarness.Unique("Lesson"));

        var content = await _h.ServiceAsync<Core.Modules.LessonContent.Abstractions.ILessonContentService,
            Core.Modules.LessonContent.Responses.LessonContentResponse>(
            s => s.CreateAsync(new Core.Modules.LessonContent.Requests.LessonContentCreateRequest
            {
                LessonId = lessonId,
                Title = NodeTestHarness.Unique("E2E"),
                Type = Core.Modules.LessonContent.Entities.LessonContentType.Video,
                IsActive = true,
                Path = relativeKey
            }));

        return await _h.DbAsync(db => db.LessonContents.Where(c => c.Id == content.Id)
            .Select(c => c.Video!.Id).FirstAsync());
    }

    [Fact]
    public async Task A_real_worker_converts_and_publishes_a_video()
    {
        if (Executable("ffmpeg") is null || Executable("ffprobe") is null) return;

        var videoId = await SeedRealVideoAsync();

        // Test öz ayar faylını işlədir — istifadəçinin real konfiqurasiyası toxunulmaz qalır.
        var settings = new WorkerSettings
        {
            ApiUrl = factory.Server.BaseAddress.ToString().TrimEnd('/'),
            NodeName = NodeTestHarness.Unique("e2e-node"),
            CacheDirectory = _cache,
            SettingsPath = Path.Combine(_cache, "settings.json")
        };

        settings.Limits.MaxConcurrentJobs = 2;

        var log = new WorkerLog();

        using var http = factory.CreateClient();
        await using var engine = new WorkerEngine(settings, log, http: http);

        var code = await _h.ServiceAsync<Core.Modules.Nodes.Abstractions.INodeService, string>(async s =>
            (await s.CreateCodeAsync(new CreateRegistrationCodeRequest())).Code);

        Assert.True(await engine.RegisterAsync(code));

        await engine.StartAsync();

        try
        {
            var ready = await WaitForAsync(
                async () => (await _h.VideoAsync(videoId))?.ConversionStatus == VideoStatus.Ready,
                TimeSpan.FromMinutes(4));

            var video = await _h.VideoAsync(videoId);

            Assert.True(ready,
                $"Video hazır olmadı. Status: {video?.ConversionStatus}, xəta: {video?.ConversionError}. "
                + $"Jurnal: {string.Join(" | ", log.Snapshot().TakeLast(10).Select(e => e.Message))}");

            Assert.NotNull(video!.PlaybackUrl);
            Assert.NotNull(video.Thumbnail);
            Assert.Null(video.ConversionError);

            var master = await _h.ReadAsync(video.PlaybackUrl!);

            Assert.NotNull(master);
            Assert.StartsWith("#EXTM3U", master);

            // Mənbə 720p-dir → 1080p upscale edilməməli, qalan pillələr yazılmalıdır.
            Assert.Contains("720p/index.m3u8", master);
            Assert.Contains("480p/index.m3u8", master);
            Assert.Contains("360p/index.m3u8", master);
            Assert.DoesNotContain("1080p/index.m3u8", master);

            // Hər variantın playlist-i öz seqmentlərinə istinad edir və onlar mövcuddur.
            foreach (var resolution in new[] { "360p", "480p", "720p" })
            {
                var prefix = video.PlaybackUrl![..video.PlaybackUrl!.LastIndexOf('/')];
                var variant = await _h.ReadAsync($"{prefix}/{resolution}/index.m3u8");

                Assert.NotNull(variant);
                Assert.Contains("#EXT-X-ENDLIST", variant);

                var segment = variant!
                    .Split('\n')
                    .Select(line => line.Trim())
                    .First(line => line.EndsWith(".ts", StringComparison.Ordinal));

                Assert.True(await _h.ExistsAsync($"{prefix}/{resolution}/{segment}"));
            }

            // Mənbədən böyük pillə ləğv edilib, qalanları tamamlanıb.
            var jobs = await _h.JobsAsync(videoId);

            Assert.All(jobs.Where(j => j.Resolution != "1080p"), job => Assert.Equal(JobStatus.Completed, job.Status));
            Assert.All(jobs.Where(j => j.Resolution == "1080p"), job =>
            {
                Assert.Equal(JobStatus.Cancelled, job.Status);
                Assert.True(job.Skipped);
            });
        }
        finally
        {
            await engine.StopAsync();
        }
    }

    private async Task<bool> WaitForAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await condition()) return true;

            await Task.Delay(1000);
        }

        return false;
    }
}
