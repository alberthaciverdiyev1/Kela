using System.Globalization;
using System.Text;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Media.Abstractions;
using Kela.Core.Modules.Media.Entities;
using Kela.Core.Modules.Media.Requests;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Nodes.Shared.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kela.Core.Modules.Nodes.Services;

using VideoJob = Kela.Core.Modules.Nodes.Entities.VideoJob;
using WorkerNode = Kela.Core.Modules.Nodes.Entities.WorkerNode;

public sealed class VideoJobLifecycle(
    AppDbContext db,
    IVideoJobScheduler scheduler,
    IFileStorage storage,
    ITenantFolder folders,
    IVideoProcessingService videoProcessing,
    Kela.Core.Modules.Subscription.Abstractions.ISubscriptionService subscription,
    IHttpContextAccessor accessor,
    IOptions<NodeOptions> options,
    ILogger<VideoJobLifecycle> logger) : IVideoJobLifecycle
{
    private const int MaxUploadUrlsPerRequest = 2000;

    private static readonly string[] WorkerStatuses =
        [JobStatus.Assigned, JobStatus.Downloading, JobStatus.Processing, JobStatus.Uploading];

    private NodeOptions Settings => options.Value;

    public async Task<JobOperationResult> ReportProgressAsync(
        WorkerNode node, long jobId, JobProgressRequest request, CancellationToken ct = default)
    {
        var job = await FindJobAsync(node, jobId, ct);
        if (job is null) return JobOperationResult.NotFound();

        if (!Owned(job, request.AttemptToken)) return JobOperationResult.Conflict("Attempt is stale");
        if (JobStatus.IsTerminal(job.Status)) return JobOperationResult.Conflict("Job already finished");

        if (WorkerStatuses.Contains(request.Status)) job.Status = request.Status;

        job.Progress = Math.Clamp(request.Progress, 0, 1);
        if (request.Fps is > 0) job.EncodingFps = request.Fps;
        if (!string.IsNullOrWhiteSpace(request.Message)) job.ErrorMessage = Truncate(request.Message, 1000);

        if (request.Media is { Height: > 0 } media)
        {
            await ApplySourceMediaAsync(job, media, ct);
        }

        if (request.ExtendLease)
        {
            job.LeaseExpiresAt = DateTimeOffset.UtcNow.AddMinutes(Settings.LeaseMinutes);
        }

        await db.SaveChangesAsync(ct);
        return JobOperationResult.Ok();
    }

    /// <summary>
    /// Mənbə ölçüləri məlum olanda artıq pillələri ləğv edirik — mənbədən böyük
    /// çıxış kodlaşdırmaq (upscale) nə keyfiyyət verir, nə vaxt qazandırır.
    /// </summary>
    private async Task ApplySourceMediaAsync(VideoJob job, SourceMediaInfo media, CancellationToken ct)
    {
        var siblings = await db.VideoJobs.IgnoreQueryFilters()
            .Where(j => j.VideoId == job.VideoId && j.OutputPrefix == job.OutputPrefix)
            .ToListAsync(ct);

        if (siblings.Any(j => j.SourceHeight is not null)) return;

        foreach (var sibling in siblings)
        {
            sibling.SourceHeight = media.Height;
            sibling.SourceWidth = media.Width;
            sibling.SourceFps = media.Fps;
            sibling.HasAudio = media.HasAudio;
        }

        var keep = siblings.Where(j => j.TargetHeight <= media.Height).ToList();
        if (keep.Count == 0) keep = [siblings.OrderBy(j => j.TargetHeight).First()];

        foreach (var sibling in siblings.Where(j => !keep.Contains(j) && !JobStatus.IsTerminal(j.Status)))
        {
            sibling.Status = JobStatus.Cancelled;
            sibling.Skipped = true;
            sibling.CompletedAt = DateTimeOffset.UtcNow;
            sibling.Progress = 0;
            sibling.WorkerNodeId = null;
            sibling.AttemptToken = null;
            sibling.LeaseExpiresAt = null;
            sibling.ErrorMessage = $"Skipped: source is {media.Height}p";
        }

        var video = await db.Videos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.Id == job.VideoId && v.DeletedAt == null, ct);

        if (video is not null && media.DurationSeconds is > 0 && video.DurationSeconds is null)
        {
            video.DurationSeconds = media.DurationSeconds;
        }
    }

    public async Task<(JobOperationResult Result, JobUploadUrlResponse? Urls)> CreateUploadUrlsAsync(
        WorkerNode node, long jobId, JobUploadUrlRequest request, CancellationToken ct = default)
    {
        var job = await FindJobAsync(node, jobId, ct);
        if (job is null) return (JobOperationResult.NotFound(), null);

        if (!Owned(job, request.AttemptToken)) return (JobOperationResult.Conflict("Attempt is stale"), null);
        if (JobStatus.IsTerminal(job.Status)) return (JobOperationResult.Conflict("Job already finished"), null);

        if (request.Keys.Count == 0) return (JobOperationResult.Invalid("No keys requested"), null);
        if (request.Keys.Count > MaxUploadUrlsPerRequest)
        {
            return (JobOperationResult.Invalid($"At most {MaxUploadUrlsPerRequest} keys per request"), null);
        }

        var expiry = TimeSpan.FromMinutes(Settings.UploadUrlExpiryMinutes);
        var urls = new Dictionary<string, string>(request.Keys.Count);
        var proxy = false;

        foreach (var relative in request.Keys)
        {
            var key = NodePaths.Combine(job.OutputPrefix, relative);

            // Worker yalnız öz variant qovluğuna və thumbnail-ə yaza bilər.
            if (key is null
                || !NodePaths.IsInsidePrefix(job.OutputPrefix, key)
                || (relative != ThumbnailRelative && !relative.Replace('\\', '/').TrimStart('/').StartsWith($"{job.Resolution}/", StringComparison.Ordinal)))
            {
                return (JobOperationResult.Invalid($"Key outside the job's output folder: {relative}"), null);
            }

            var signed = storage.CreatePresignedPutUrl(key, ContentTypeFor(key), expiry);

            if (signed is null)
            {
                proxy = true;
                signed = ProxyUploadUrl(job.Id, relative, job.AttemptToken!);
            }

            urls[relative] = signed!;
        }

        if (proxy) job.Status = JobStatus.Uploading;

        await db.SaveChangesAsync(ct);
        return (JobOperationResult.Ok(), new JobUploadUrlResponse
        {
            Urls = urls,
            Proxy = proxy,
            ExpiresSeconds = (int)expiry.TotalSeconds
        });
    }

    public async Task<JobOperationResult> CompleteAsync(
        WorkerNode node, long jobId, JobCompleteRequest request, CancellationToken ct = default)
    {
        var job = await FindJobAsync(node, jobId, ct);
        if (job is null) return JobOperationResult.NotFound();

        if (!Owned(job, request.AttemptToken)) return JobOperationResult.Conflict("Attempt is stale");
        if (JobStatus.IsTerminal(job.Status)) return JobOperationResult.Conflict("Job already finished");

        job.Status = JobStatus.Validating;
        await db.SaveChangesAsync(ct);

        var validation = await ValidateOutputsAsync(job, request, ct);
        if (!validation.IsOk)
        {
            job.Status = JobStatus.Processing;
            await db.SaveChangesAsync(ct);
            return validation;
        }

        job.Status = JobStatus.Completed;
        job.Progress = 1;
        job.CompletedAt = DateTimeOffset.UtcNow;
        job.EncodingFps = request.Fps ?? job.EncodingFps;
        job.Encoder = Truncate(request.Encoder, 60);
        job.AttemptToken = null;
        job.LeaseExpiresAt = null;
        job.NextAttemptAt = null;
        job.ErrorMessage = null;
        job.ErrorKind = null;

        node.CompletedJobs += 1;
        if (node.ActiveJobs > 0) node.ActiveJobs -= 1;

        await db.SaveChangesAsync(ct);

        await FinalizeAsync(job.VideoId, ct);
        return JobOperationResult.Ok();
    }

    /// <summary>
    /// Çıxışları storage-dan yoxlayır: variant playlisti mövcuddur, ən azı bir seqment var
    /// və playlist-in istinad etdiyi bütün seqmentlər həqiqətən yazılıb. Natamam HLS
    /// yayıma çıxmasın deyə bu yoxlama vacibdir.
    /// </summary>
    private async Task<JobOperationResult> ValidateOutputsAsync(VideoJob job, JobCompleteRequest request, CancellationToken ct)
    {
        var folder = NodePaths.VariantFolder(job.OutputPrefix, job.Resolution);
        var listed = (await storage.ListAsync(folder + "/", ct)).ToHashSet(StringComparer.Ordinal);

        var playlistKey = NodePaths.Combine(job.OutputPrefix, request.VariantKey is { Length: > 0 } vk
            ? vk
            : NodePaths.VariantKey(job.OutputPrefix, job.Resolution));

        if (playlistKey is null || !NodePaths.IsInsidePrefix(job.OutputPrefix, playlistKey))
        {
            return JobOperationResult.Invalid("Variant playlist key is invalid");
        }

        if (!listed.Contains(playlistKey))
        {
            return JobOperationResult.Invalid($"Variant playlist was not uploaded: {playlistKey}");
        }

        var playlist = await storage.ReadTextAsync(playlistKey, ct);
        if (string.IsNullOrWhiteSpace(playlist))
        {
            return JobOperationResult.Invalid("Variant playlist is empty");
        }

        var referenced = ParseSegmentReferences(playlist);
        if (referenced.Count == 0)
        {
            return JobOperationResult.Invalid("Variant playlist references no segments");
        }

        var missing = referenced
            .Select(name => $"{folder}/{name}")
            .Where(key => !listed.Contains(key))
            .Take(5)
            .ToList();

        if (missing.Count > 0)
        {
            return JobOperationResult.Invalid($"Missing segments: {string.Join(", ", missing)}");
        }

        // Worker-ın bildirdiyi hər obyekt həqiqətən mövcud olmalıdır.
        foreach (var relative in request.Objects)
        {
            var key = NodePaths.Combine(job.OutputPrefix, relative);
            if (key is null || !NodePaths.IsInsidePrefix(job.OutputPrefix, key))
            {
                return JobOperationResult.Invalid($"Reported object is outside the output folder: {relative}");
            }

            if (!await storage.ExistsAsync(key, ct))
            {
                return JobOperationResult.Invalid($"Reported object is missing: {relative}");
            }
        }

        return JobOperationResult.Ok();
    }

    private static List<string> ParseSegmentReferences(string playlist)
    {
        var names = new List<string>();

        foreach (var raw in playlist.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            // Yalnız fayl adı qəbul edilir — alt qovluq və ".." rədd olunur.
            if (line.Contains('/') || line.Contains('\\') || line.Contains("..", StringComparison.Ordinal)) continue;
            if (!line.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)) continue;

            names.Add(line);
        }

        return names;
    }

    public async Task<JobOperationResult> FailAsync(
        WorkerNode node, long jobId, JobFailRequest request, CancellationToken ct = default)
    {
        var job = await FindJobAsync(node, jobId, ct);
        if (job is null) return JobOperationResult.NotFound();

        if (!Owned(job, request.AttemptToken)) return JobOperationResult.Conflict("Attempt is stale");
        if (JobStatus.IsTerminal(job.Status)) return JobOperationResult.Conflict("Job already finished");

        var kind = JobErrorKind.All.Contains(request.Kind) ? request.Kind : JobErrorKind.Transient;
        var permanent = kind == JobErrorKind.Permanent || job.Attempts >= job.MaxAttempts;

        job.ErrorMessage = Truncate(request.Error, 1000) ?? "Conversion failed";
        job.ErrorKind = kind;
        job.AttemptToken = null;
        job.LeaseExpiresAt = null;
        job.Progress = 0;

        // Yenidən kuyruğa düşərsə sahibi qalmamalıdır; qalıcı uğursuzluqda hansı
        // cihazda alınmadığı bilinsin deyə saxlanılır.
        if (!permanent) job.WorkerNodeId = null;

        node.FailedJobs += 1;
        if (node.ActiveJobs > 0) node.ActiveJobs -= 1;

        if (permanent)
        {
            job.Status = JobStatus.Failed;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.NextAttemptAt = null;
        }
        else
        {
            // Exponential backoff: qısa nasazlıqlar üçün təkrar cəhd, amma tələsik yox.
            job.Status = JobStatus.Pending;
            job.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(10 * Math.Pow(2, Math.Min(job.Attempts, 6)));
        }

        await db.SaveChangesAsync(ct);

        if (permanent) await CleanupFailedOutputsAsync(job, ct);

        await FinalizeAsync(job.VideoId, ct);
        return JobOperationResult.Ok();
    }

    private async Task CleanupFailedOutputsAsync(VideoJob job, CancellationToken ct)
    {
        try
        {
            var folder = NodePaths.VariantFolder(job.OutputPrefix, job.Resolution);

            foreach (var key in await storage.ListAsync(folder + "/", ct))
            {
                if (NodePaths.IsInsidePrefix(job.OutputPrefix, key)) await storage.DeleteAsync(key, ct);
            }
        }
        catch (Exception ex)
        {
            // Təmizlik alınmasa konversiya nəticəsi dəyişmir — sadəcə jurnalda qalsın.
            logger.LogWarning(ex, "Failed to clean outputs of job {JobId}", job.Id);
        }
    }

    public async Task<JobOperationResult> FinalizeAsync(long videoId, CancellationToken ct = default)
    {
        var video = await db.Videos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.Id == videoId && v.DeletedAt == null, ct);

        if (video is null) return JobOperationResult.NotFound();

        var tenantFolder = await folders.ResolveAsync(video.TenantId, ct);
        var prefix = NodePaths.OutputPrefix(video.Path, video.Id, video.Title, video.ConversionVersion, tenantFolder);

        var jobs = await db.VideoJobs.IgnoreQueryFilters()
            .Where(j => j.VideoId == videoId && j.OutputPrefix == prefix)
            .ToListAsync(ct);

        if (jobs.Count == 0)
        {
            // Prefiks mənbə açarından törədilir; tenant qovluğu sonradan dəyişsə
            // hesablanan prefiks tapşırıqlardakı ilə uyğun gəlməyə bilər. O halda
            // videonun ən son versiyasını işlədən tapşırıqlar götürülür ki, video
            // "işlənir" vəziyyətində ilişib qalmasın.
            jobs = await db.VideoJobs.IgnoreQueryFilters()
                .Where(j => j.VideoId == videoId)
                .OrderByDescending(j => j.Id)
                .Take(16)
                .ToListAsync(ct);

            var latest = jobs.FirstOrDefault()?.OutputPrefix;

            jobs = latest is null ? [] : [.. jobs.Where(j => j.OutputPrefix == latest)];
            prefix = latest ?? prefix;
        }

        if (jobs.Count == 0) return JobOperationResult.Ok();
        if (jobs.Any(j => JobStatus.IsActive(j.Status))) return JobOperationResult.Ok();

        var completed = jobs.Where(j => j.Status == JobStatus.Completed && !j.Skipped)
            .OrderBy(j => j.TargetHeight)
            .ToList();

        var failures = jobs.Where(j => j.Status == JobStatus.Failed).ToList();

        if (failures.Count > 0 || completed.Count == 0)
        {
            var error = failures.FirstOrDefault()?.ErrorMessage ?? "Konversiya tamamlanmadı";

            // Xəta keçicidirsə (şəbəkə kəsilməsi, disk, cihazın sönməsi) video dərhal
            // "failed" olmamalıdır: bütün pillələr yenidən növbəyə salınır və bir neçə
            // dövr təkrar sınanır. Əks halda bir anlıq nasazlıq videonu əbədi
            // "bozulmuş" vəziyyətdə qoyurdu.
            if (failures.Count > 0 && await scheduler.RequeueVideoAsync(videoId, error, ct))
            {
                return JobOperationResult.Invalid(error);
            }

            // Köhnə uğurlu versiya silinmir — istifadəçi əvvəlki m3u8-i oynatmağa davam edir.
            await videoProcessing.MarkProcessedAsync(videoId, new VideoProcessedRequest
            {
                SourcePath = video.Path,
                Status = VideoStatus.Failed,
                Error = error
            }, ct);

            return JobOperationResult.Invalid(error);
        }

        // Son müdafiə xətti: tamamlanmış saysaq da playlist-in həqiqətən
        // storage-da olduğunu yoxlayırıq — yalançı nəticə master-a düşməsin.
        foreach (var variant in completed.ToList())
        {
            if (await storage.ExistsAsync($"{prefix}/{variant.Resolution}/index.m3u8", ct)) continue;

            completed.Remove(variant);
            variant.Status = JobStatus.Failed;
            variant.ErrorMessage = "Çıxış storage-da tapılmadı";
            variant.ErrorKind = JobErrorKind.Permanent;
        }

        await db.SaveChangesAsync(ct);

        if (completed.Count == 0)
        {
            await videoProcessing.MarkProcessedAsync(videoId, new VideoProcessedRequest
            {
                SourcePath = video.Path,
                Status = VideoStatus.Failed,
                Error = "Heç bir variant yayıma hazır deyil"
            }, ct);

            return JobOperationResult.Invalid("No variant could be published");
        }

        // Paralel iki worker eyni anda finalize edə bilər — video artıq yayımdadırsa
        // master yenidən yazılmır və video dəqiqəsi ikinci dəfə sayılmır.
        if (video.ConversionStatus == VideoStatus.Ready && video.PlaybackUrl is not null)
        {
            return JobOperationResult.Ok();
        }

        var masterKey = NodePaths.MasterKey(prefix);
        var thumbnailKey = NodePaths.ThumbnailKey(prefix);
        var master = BuildMasterPlaylist(completed);

        await storage.UploadTextAsync(masterKey, master, "application/vnd.apple.mpegurl", ct);

        var thumbnail = await storage.ExistsAsync(thumbnailKey, ct) ? thumbnailKey : null;

        // Uğurlu konversiyadan sonra cəhd büdcəsi yenidən dolsun — gələcək
        // yenidən konversiya yenidən tam cəhd haqqı əldə etsin.
        video.ConversionAttempts = 0;
        video.ConversionError = null;
        await db.SaveChangesAsync(ct);

        await videoProcessing.MarkProcessedAsync(videoId, new VideoProcessedRequest
        {
            SourcePath = video.Path,
            PlaybackUrl = masterKey,
            Thumbnail = thumbnail,
            DurationSeconds = video.DurationSeconds,
            Status = VideoStatus.Ready
        }, ct);

        // Abunəlik limiti: mənbə videonun faktiki müddəti qədər (yuxarı yuvarlaqlaşdırılır).
        var minutes = video.DurationSeconds is int seconds && seconds > 0 ? (seconds + 59) / 60 : 0;
        if (minutes > 0)
        {
            await subscription.RecordVideoMinutesAsync(video.TenantId, minutes, ct);
        }

        logger.LogInformation("Published HLS version {Version} for video {VideoId}", video.ConversionVersion, videoId);
        return JobOperationResult.Ok();
    }

    /// <summary>Master playlist yalnız tamamlanmış variantlardan qurulur.</summary>
    private static string BuildMasterPlaylist(List<VideoJob> variants)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#EXTM3U");
        builder.AppendLine("#EXT-X-VERSION:3");
        builder.AppendLine("#EXT-X-INDEPENDENT-SEGMENTS");

        foreach (var variant in variants)
        {
            var audioKbps = variant.TargetHeight >= 1080 ? 192 : variant.TargetHeight >= 480 ? 128 : 96;
            var bandwidth = (variant.TargetBitrateKbps + audioKbps) * 1000;
            var codec = variant.TargetHeight >= 1080 ? "avc1.640028,mp4a.40.2" : "avc1.4d401f,mp4a.40.2";

            var attributes = new List<string>
            {
                $"BANDWIDTH={bandwidth}",
                $"AVERAGE-BANDWIDTH={bandwidth}",
                $"CODECS=\"{codec}\""
            };

            var width = EstimateWidth(variant);
            if (width > 0) attributes.Add($"RESOLUTION={width}x{variant.TargetHeight}");

            builder.AppendLine($"#EXT-X-STREAM-INF:{string.Join(',', attributes)}");
            builder.AppendLine($"{variant.Resolution}/index.m3u8");
        }

        return builder.ToString();
    }

    /// <summary>Aspect ratio qorunur; mənbə ölçüləri bilinmirsə RESOLUTION yazılmır.</summary>
    private static int EstimateWidth(VideoJob job)
    {
        if (job.SourceWidth is not > 0 || job.SourceHeight is not > 0) return 0;

        var width = (int)Math.Round(job.SourceWidth.Value * (double)job.TargetHeight / job.SourceHeight.Value);
        return Math.Max(2, width - width % 2);
    }

    private Task<VideoJob?> FindJobAsync(WorkerNode node, long jobId, CancellationToken ct)
        // Job yalnız onu kilidləyən cihaz tərəfindən idarə olunur.
        => db.VideoJobs.IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.WorkerNodeId == node.Id && j.DeletedAt == null, ct);

    private static bool Owned(VideoJob job, string attemptToken)
        => !string.IsNullOrEmpty(job.AttemptToken)
           && !string.IsNullOrEmpty(attemptToken)
           && string.Equals(job.AttemptToken, attemptToken, StringComparison.Ordinal);

    private string? ProxyUploadUrl(long jobId, string relativeKey, string attemptToken)
    {
        var context = accessor.HttpContext;
        var baseUrl = context is null
            ? string.Empty
            : $"{context.Request.Scheme}://{context.Request.Host}";

        // Fencing token-i URL-ə bağlanır: URL başqa cəhdə keçsə, yükləmə rədd olunur.
        return $"{baseUrl}/api/nodes/jobs/{jobId}/object"
             + $"?key={Uri.EscapeDataString(relativeKey)}&token={Uri.EscapeDataString(attemptToken)}";
    }

    private static string ContentTypeFor(string key) => Path.GetExtension(key).ToLowerInvariant() switch
    {
        ".m3u8" => "application/vnd.apple.mpegurl",
        ".ts" => "video/mp2t",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".vtt" => "text/vtt",
        _ => "application/octet-stream"
    };

    private const string ThumbnailRelative = "thumb.jpg";

    private static string? Truncate(string? value, int max)
        => string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value : value[..max];
}
