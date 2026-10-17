using System.Globalization;
using Kela.Nodes.Shared.Contracts;
using Kela.Nodes.Worker.Device;
using Kela.Nodes.Worker.Encoding;

namespace Kela.Nodes.Worker.Jobs;

/// <summary>UI-ya göstərilən iş vəziyyəti.</summary>
public sealed class RunningJob(JobAssignment assignment)
{
    public long JobId { get; } = assignment.JobId;

    public long VideoId { get; } = assignment.VideoId;

    public string Resolution { get; } = assignment.Resolution;

    public string AttemptToken { get; } = assignment.AttemptToken;

    public string Stage { get; set; } = JobStatus.Assigned;

    public double Progress { get; set; }

    public double Fps { get; set; }

    public double Speed { get; set; }

    public string? Message { get; set; }

    public int? EtaSeconds { get; set; }

    /// <summary>Hansı kodlaşdırma zolağında işləyir (məs. "GPU 0 · nvenc").</summary>
    public string Lane { get; set; } = "—";

    /// <summary>Oxunaqlı mənbə adı (məs. ScreenRecording_08-08-2025.mp4).</summary>
    public string SourceName { get; set; } = "—";

    public long DownloadedBytes { get; set; }

    public long TotalBytes { get; set; }

    /// <summary>Mənbənin elan olunmuş ölçüsü (koordinatordan) — böyük videonu tanımaq üçün.</summary>
    public long SourceBytes { get; } = assignment.SourceBytes ?? 0;

    /// <summary>Ölçüsünə görə böyük video sayılır.</summary>
    public bool IsBigVideo(long threshold) => SourceBytes >= threshold;

    public double DownloadMegabytesPerSecond { get; set; }

    /// <summary>Endirmə mətnı: "12.4 / 320.0 MB · 8.3 MB/s".</summary>
    public string TransferText => TotalBytes > 0
        ? $"{DownloadedBytes / 1024d / 1024d:0.#} / {TotalBytes / 1024d / 1024d:0.#} MB"
        : DownloadedBytes > 0 ? $"{DownloadedBytes / 1024d / 1024d:0.#} MB" : "—";

    public string TransferSpeedText => DownloadMegabytesPerSecond > 0
        ? $"{DownloadMegabytesPerSecond:0.#} MB/s"
        : "—";

    /// <summary>Mənbə buraxıldıqdan sonra dəqiqləşir — yüngül iş paralel artırıla bilər.</summary>
    public bool IsLight { get; set; }

    public int SourceSeconds { get; set; }

    public int TargetHeight { get; set; }

    public DateTimeOffset StartedAt { get; } = DateTimeOffset.Now;

    public NodeJobReport ToReport() => new()
    {
        JobId = JobId,
        AttemptToken = AttemptToken,
        Status = Stage,
        Progress = Progress,
        Message = Message
    };
}

public sealed record JobOutcome(bool Success, string? Error, string Kind);

/// <summary>Koordinator bu cəhdi artıq tanımır (lease itdi) — iş dərhal buraxılmalıdır.</summary>
public sealed class LeaseLostException : Exception
{
    public LeaseLostException() : base("Lease lost")
    {
    }
}

/// <summary>
/// Bir tapşırığın tam dövriyyəsi: endir → analiz et → kodlaşdır → yüklə → təsdiqlə.
/// </summary>
public sealed class JobRunner(
    WorkerSettings settings,
    CoordinatorClient coordinator,
    WorkerStorage storage,
    HlsEncoder encoder,
    MediaProbe probe,
    WorkerLog log,
    SourceCache sources,
    ProfileSettings? profile = null)
{
    private const string ThumbnailName = "thumb.jpg";

    private static readonly TimeSpan ProgressInterval = TimeSpan.FromSeconds(5);

    public async Task<JobOutcome> RunAsync(
        JobAssignment assignment,
        EncoderCapabilities capabilities,
        EncoderLane lane,
        RunningJob state,
        string nodeToken,
        CancellationToken ct = default)
    {
        var jobDirectory = Path.Combine(settings.CacheDirectory, "jobs", $"{assignment.JobId}-{assignment.AttemptToken[..8]}");

        try
        {
            Directory.CreateDirectory(jobDirectory);

            state.Stage = JobStatus.Downloading;

            state.SourceName = Path.GetFileName(assignment.SourceKey);

            var progress = new Progress<TransferProgress>(transfer =>
            {
                state.DownloadedBytes = transfer.Bytes;
                state.TotalBytes = transfer.TotalBytes;
                state.DownloadMegabytesPerSecond = transfer.MegabytesPerSecond;
                state.Progress = transfer.TotalBytes > 0 ? (double)transfer.Bytes / transfer.TotalBytes : 0;
                state.Message = $"{state.SourceName}: {state.TransferText} · {state.TransferSpeedText}";
            });

            var download = await sources.AcquireAsync(
                assignment.VideoId, assignment.SourceKey, assignment.SourceUrl, progress, ct);

            if (download.Path is null)
            {
                var message = download.Error ?? "Mənbə fayl endirilə bilmədi";

                // Mənbə storage-da yoxdursa təkrar cəhd mənasızdır.
                return new JobOutcome(false, message,
                    download.Permanent ? JobErrorKind.Permanent : JobErrorKind.Transient);
            }

            var source = download.Path;

            var media = await probe.ProbeAsync(source, ct);
            if (media is null || media.Width <= 0 || media.Height <= 0)
            {
                return new JobOutcome(false, "Mənbə fayl oxunmur və ya video axını yoxdur", JobErrorKind.Permanent);
            }

            // Mənbə ölçüləri koordinatora bildirilir — o, lazımsız pillələri ləğv edir.
            await ReportAsync(assignment, state, new JobProgressRequest
            {
                AttemptToken = assignment.AttemptToken,
                Status = JobStatus.Processing,
                Progress = 0,
                Media = media.ToSourceMediaInfo()
            }, ct);

            var failure = await EncodeAsync(assignment, media, lane, jobDirectory, source, state, ct);
            if (failure is not null) return failure;

            failure = await UploadAsync(assignment, jobDirectory, state, nodeToken, ct);
            if (failure is not null) return failure;

            var complete = await coordinator.CompleteAsync(assignment.JobId, new JobCompleteRequest
            {
                AttemptToken = assignment.AttemptToken,
                DurationSeconds = media.DurationSeconds,
                ThumbnailKey = GenerateThumbnail(assignment) ? ThumbnailName : null,
                VariantKey = $"{assignment.Resolution}/index.m3u8",
                Objects = [.. CollectFiles(assignment, jobDirectory).Select(f => f.Key)],
                Fps = state.Fps > 0 ? state.Fps : null,
                Encoder = lane.Name
            }, ct);

            if (complete.IsOk)
            {
                state.Stage = JobStatus.Completed;
                state.Progress = 1;
                return new JobOutcome(true, null, JobErrorKind.Transient);
            }

            // Koordinator nəticəni qəbul etmədi (məsələn seqment çatışmır) — təkrar cəhd edilsin.
            return Transient(complete.Message ?? "Nəticə qəbul edilmədi", state);
        }
        catch (LeaseLostException)
        {
            throw;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or HttpRequestException or UnauthorizedAccessException)
        {
            log.Warn("job", $"İş {assignment.JobId}: {ex.Message}");
            return Transient(ex.Message, state);
        }
        finally
        {
            if (settings.CleanupAfterJob) TryDelete(jobDirectory);
        }
    }

    private async Task<JobOutcome?> EncodeAsync(
        JobAssignment assignment, MediaInfo media, EncoderLane lane,
        string jobDirectory, string source, RunningJob state, CancellationToken ct)
    {
        state.Stage = JobStatus.Processing;
        state.Progress = 0;

        var lastReport = DateTimeOffset.MinValue;

        var progress = new Progress<EncodeProgress>(update =>
        {
            lock (state)
            {
                if (update.Fraction >= 0) state.Progress = update.Fraction;
                if (update.Fps is > 0) state.Fps = update.Fps.Value;
                if (update.Speed is > 0) state.Speed = update.Speed.Value;

                if (state.Speed > 0 && media.DurationSeconds > 0)
                {
                    state.EtaSeconds = (int)Math.Max(0, media.DurationSeconds * (1 - state.Progress) / state.Speed);
                }

                state.Message = $"{assignment.Resolution}: {(state.Progress * 100).ToString("0", CultureInfo.InvariantCulture)}%"
                              + (state.Fps > 0 ? $" · {state.Fps:0.#} fps" : string.Empty);
            }

            // Koordinatora seyrək bildiriş — hər yenilənmə üçün HTTP sorğusu göndərilmir.
            if (DateTimeOffset.UtcNow - lastReport < ProgressInterval) return;

            lastReport = DateTimeOffset.UtcNow;
            _ = CoordinatorProgressAsync(assignment, state);
        });

        var request = new EncodeRequest
        {
            Input = source,
            OutputDirectory = Path.Combine(jobDirectory, assignment.Resolution),
            Profile = assignment.Profile,
            Encoder = new EncoderChoice(lane.Name, lane.HwAccel, lane.Hardware),
            GpuIndex = lane.Hardware ? lane.GpuIndex : -1,
            SourceHeight = media.Height,
            SourceFps = media.Fps,
            DurationSeconds = media.DurationSeconds,
            VideoStreamIndex = media.VideoStreamIndex,
            AudioStreamIndex = media.HasAudio ? media.AudioStreamIndex : -1,
            SegmentSeconds = 8,
            Threads = lane.Hardware ? 0 : Math.Max(1, profile?.CpuThreads ?? settings.Limits.MaxCpuThreads),
            // Aparat encoder-i öz preset adlarını işlədir (p1..p7), CPU isə veryfast/fast.
            // Zolağın öz preset-i: GPU zolağı p1/p4/p6, CPU zolağı veryfast.
            Preset = lane.Preset,
            BelowNormalPriority = lane.BelowNormalPriority
        };

        var outcome = await encoder.EncodeAsync(request, progress, request.BelowNormalPriority, ct);

        if (!outcome.Success)
        {
            log.Error("job", $"İş {assignment.JobId} ({lane.Name}) kodlaşdırıla bilmədi: {outcome.Error}");

            return new JobOutcome(false, outcome.Error, ClassifyEncodeError(outcome.Error));
        }

        if (GenerateThumbnail(assignment))
        {
            await encoder.CreateThumbnailAsync(source, Path.Combine(jobDirectory, ThumbnailName),
                media.VideoStreamIndex, media.DurationSeconds, ct);
        }

        return null;
    }

    /// <summary>Thumbnail yalnız ən aşağı pillə tərəfindən yaradılır — eyni iş təkrarlanmasın.</summary>
    private static bool GenerateThumbnail(JobAssignment assignment)
        => assignment.Ladder.Count == 0 || assignment.Profile.Height == assignment.Ladder.Min(p => p.Height);

    private async Task<JobOutcome?> UploadAsync(
        JobAssignment assignment, string jobDirectory, RunningJob state, string nodeToken, CancellationToken ct)
    {
        state.Stage = JobStatus.Uploading;
        state.Progress = 0;

        var files = CollectFiles(assignment, jobDirectory);

        if (files.Count == 0)
        {
            return new JobOutcome(false, "Kodlaşdırma çıxış faylı yaratmadı", JobErrorKind.Transient);
        }

        var urls = await coordinator.UploadUrlsAsync(assignment.JobId, new JobUploadUrlRequest
        {
            AttemptToken = assignment.AttemptToken,
            Keys = [.. files.Select(f => f.Key)]
        }, ct);

        if (!urls.IsOk || urls.Data is null)
        {
            return Transient(urls.Message ?? "Yükləmə URL-ləri alınmadı", state);
        }

        // Seqmentlər playlist-dən ƏVVƏL yüklənir: yarımçıq yükləmə işlək görünən
        // playlist yaratmasın.
        var ordered = files.OrderBy(f => f.Key.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ? 1 : 0).ToList();
        var totalBytes = files.Sum(f => f.Size);
        long uploaded = 0;

        foreach (var file in ordered)
        {
            ct.ThrowIfCancellationRequested();

            if (!urls.Data.Urls.TryGetValue(file.Key, out var url))
            {
                return Transient($"Yükləmə URL-i yoxdur: {file.Key}", state);
            }

            try
            {
                await storage.UploadAsync(url, file.Path, ContentTypeFor(file.Key), urls.Data.Proxy, nodeToken, ct);
            }
            catch (HttpRequestException ex)
            {
                return Transient(ex.Message, state);
            }

            uploaded += file.Size;
            state.Progress = totalBytes > 0 ? (double)uploaded / totalBytes : 1;
            state.Message = $"Yüklənir: {file.Key}";
        }

        return null;
    }

    private List<(string Key, string Path, long Size)> CollectFiles(JobAssignment assignment, string jobDirectory)
    {
        var variantDirectory = Path.Combine(jobDirectory, assignment.Resolution);

        if (!Directory.Exists(variantDirectory)) return [];

        var files = Directory.EnumerateFiles(variantDirectory)
            .Select(path => ($"{assignment.Resolution}/{Path.GetFileName(path)}", path, new FileInfo(path).Length))
            .ToList();

        var thumbnail = Path.Combine(jobDirectory, ThumbnailName);

        if (GenerateThumbnail(assignment) && File.Exists(thumbnail))
        {
            files.Add((ThumbnailName, thumbnail, new FileInfo(thumbnail).Length));
        }

        return files;
    }

    private async Task ReportAsync(JobAssignment assignment, RunningJob state, JobProgressRequest request, CancellationToken ct)
    {
        var result = await coordinator.ProgressAsync(assignment.JobId, request, ct);

        if (result.Status is System.Net.HttpStatusCode.Conflict or System.Net.HttpStatusCode.NotFound)
        {
            // Koordinator bu cəhdi artıq tanımır — iş başqasına verilib.
            throw new LeaseLostException();
        }
    }

    private async Task CoordinatorProgressAsync(JobAssignment assignment, RunningJob state)
    {
        try
        {
            var report = state.ToReport();

            await coordinator.ProgressAsync(assignment.JobId, new JobProgressRequest
            {
                AttemptToken = report.AttemptToken,
                Status = report.Status,
                Progress = report.Progress,
                Fps = state.Fps > 0 ? state.Fps : null,
                Speed = state.Speed > 0 ? state.Speed : null,
                EtaSeconds = state.EtaSeconds,
                Message = report.Message
            });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Şəbəkə kəsildi — heartbeat dövrü vəziyyəti bərpa edəcək.
        }
    }

    private static string ContentTypeFor(string key) => Path.GetExtension(key).ToLowerInvariant() switch
    {
        ".m3u8" => "application/vnd.apple.mpegurl",
        ".ts" => "video/mp2t",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => "application/octet-stream"
    };

    private JobOutcome Transient(string error, RunningJob state)
    {
        state.Message = error;

        return new JobOutcome(false, error, JobErrorKind.Transient);
    }

    /// <summary>Mənbə ilə bağlı xətalar təkrar cəhddə düzəlmir — onlar qalıcıdır.</summary>
    private static string ClassifyEncodeError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error)) return JobErrorKind.Transient;

        string[] permanentMarkers =
        [
            "Invalid data found",
            "does not contain any stream",
            "Unsupported codec",
            "No such file",
            "Permission denied",
            "moov atom not found"
        ];

        return permanentMarkers.Any(marker => error.Contains(marker, StringComparison.OrdinalIgnoreCase))
            ? JobErrorKind.Permanent
            : JobErrorKind.Transient;
    }

    private void TryDelete(string directory)
    {
        try
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log.Debug("cache", $"Keş qovluğu silinmədi: {directory}");
        }
    }
}
