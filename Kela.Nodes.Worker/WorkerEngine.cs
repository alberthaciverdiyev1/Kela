using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using Kela.Nodes.Shared.Contracts;
using Kela.Nodes.Worker.Device;
using Kela.Nodes.Worker.Encoding;
using Kela.Nodes.Worker.Jobs;

namespace Kela.Nodes.Worker;

/// <summary>Bitmiş işin qısa tarixçəsi — UI "Jobs" ekranında göstərir.</summary>
public sealed record JobHistoryEntry(
    long JobId,
    long VideoId,
    string Resolution,
    bool Success,
    string? Error,
    DateTimeOffset FinishedAt,
    double DurationSeconds);

public enum ConnectionState
{
    Offline,
    Connecting,
    Online,
    Unauthorized
}

/// <summary>
/// Worker-in ürəyi: heartbeat göndərir, iş götürür, kodlaşdırır və nəticəni bildirir.
/// Əlaqə kəsiləndə eksponensial geri çəkilmə ilə yenidən qoşulur.
/// </summary>
public sealed class WorkerEngine : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly CoordinatorClient _coordinator;
    private readonly WorkerStorage _storage;
    private readonly FfmpegLocator _ffmpeg;
    private readonly DeviceProbe _device;

    // ffmpeg ikililəri yalnız yerini tapdıqdan sonra qurula bilər.
    private MediaProbe? _probe;
    private HlsEncoder? _encoder;
    private EncoderDetector? _detector;
    private BenchmarkRunner? _benchmark;
    private readonly IPlatformStatus _platform;
    private readonly ConcurrentDictionary<long, RunningJob> _jobs = new();
    private readonly ConcurrentDictionary<long, CancellationTokenSource> _cancellations = new();
    private readonly ConcurrentQueue<JobHistoryEntry> _history = new();

    private CancellationTokenSource? _engineCancellation;
    private Task? _loop;
    private int _consecutiveFailures;

    public WorkerEngine(WorkerSettings settings, WorkerLog log, IPlatformStatus? platform = null, HttpClient? http = null)
    {
        Settings = settings;
        Log = log;
        _platform = platform ?? new NullPlatformStatus();

        // Böyük faylların endirilməsi/yüklənməsi üçün ümumi timeout qoyulmur —
        // ləğv yalnız CancellationToken ilə idarə olunur.
        _http = http ?? new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        _http.Timeout = Timeout.InfiniteTimeSpan;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Kela-Nodes/1.0");

        _coordinator = new CoordinatorClient(_http, settings, log);
        _storage = new WorkerStorage(_http, log);
        _ffmpeg = new FfmpegLocator(settings, log);
        _device = new DeviceProbe(_platform, log);
    }

    public WorkerSettings Settings { get; }

    public WorkerLog Log { get; }

    public ConnectionState Connection { get; private set; } = ConnectionState.Offline;

    public string? LastError { get; private set; }

    public bool IsRunning => _loop is { IsCompleted: false };

    public NodeDeviceInfo? Device { get; private set; }

    public EncoderCapabilities? Encoders { get; private set; }

    public NodeBenchmarkResult? BenchmarkResult { get; private set; }

    public bool FfmpegAvailable { get; private set; }

    /// <summary>Cihaz üçün seçilmiş resurs profili (auto seçilmişsə, həll olunmuş halı).</summary>
    public ProfileSettings? Profile { get; private set; }

    /// <summary>Limitlər səbəbindən iş götürülmür (batareya, şarj, Wi-Fi, temperatur).</summary>
    public string? PausedReason { get; private set; }

    public IReadOnlyCollection<RunningJob> Jobs => [.. _jobs.Values.OrderBy(j => j.JobId)];

    /// <summary>Son 100 bitmiş iş (ən yenisi əvvəldə).</summary>
    public IReadOnlyList<JobHistoryEntry> History => [.. _history.Reverse()];

    private void Remember(JobHistoryEntry entry)
    {
        _history.Enqueue(entry);

        while (_history.Count > 100 && _history.TryDequeue(out _))
        {
        }
    }

    public event Action? StateChanged;

    private void Notify() => StateChanged?.Invoke();

    /// <summary>ffmpeg-i tapır, cihazı yoxlayır və encoder-ləri real sınaqla təsdiqləyir.</summary>
    public async Task<bool> PrepareAsync(CancellationToken ct = default)
    {
        FfmpegAvailable = await _ffmpeg.LocateAsync(ct);
        Notify();

        if (!FfmpegAvailable)
        {
            LastError = "ffmpeg tapılmadı";
            return false;
        }

        _probe ??= new MediaProbe(_ffmpeg.FfprobePath!);
        _encoder ??= new HlsEncoder(_ffmpeg.FfmpegPath!, Log);
        _detector ??= new EncoderDetector(_ffmpeg.FfmpegPath!, Log);
        _benchmark ??= new BenchmarkRunner(_ffmpeg.FfmpegPath!, Log);

        if (Device is null)
        {
            Device = await _device.CollectAsync(Settings.CacheDirectory, ct);
        }

        if (Encoders is null)
        {
            Encoders = await _detector!.DetectAsync(Settings.PreferredEncoder, ct);

            Device.Encoders = [.. Encoders.Names];
            Device.HardwareAcceleration = [.. Encoders.HardwareAcceleration];
            Device.HardwareEncoding = Encoders.Chosen.Hardware;
            Device.FfmpegVersion = Path.GetFileName(_ffmpeg.FfmpegPath);
            Device.FfprobeVersion = Path.GetFileName(_ffmpeg.FfprobePath);
        }

        Profile = ProfileAdvisor.Resolve(Settings.Limits.Profile, Device, Encoders, Settings.Limits);
        Log.Info("worker", $"Resurs profili: {Profile.Profile} — {Profile.Description}");

        // Cihaz ilk dəfə qoşulanda (və ya encoder dəyişəndə) real encoding sürəti ölçülür —
        // koordinator cihazları bu balla müqayisə edir.
        if (Settings.Benchmark is { } stored && stored.Encoder == Encoders.Chosen.Name)
        {
            BenchmarkResult = stored;
        }
        else
        {
            await MeasureAsync(ct);
        }

        Notify();
        return true;
    }

    /// <summary>İstifadəçi əl ilə yenidən ölçmək istədikdə.</summary>
    public async Task<NodeBenchmarkResult> RunBenchmarkAsync(CancellationToken ct = default)
    {
        if (Encoders is null) await PrepareAsync(ct);

        await MeasureAsync(ct);
        Notify();

        return BenchmarkResult!;
    }

    private async Task MeasureAsync(CancellationToken ct)
    {
        BenchmarkResult = await _benchmark!.RunAsync(Encoders!.Chosen, ct);

        Settings.Benchmark = BenchmarkResult;
        Settings.Save();
    }

    /// <summary>Birdəfəlik qeydiyyat kodu ilə cihazı koordinatora tanıdır.</summary>
    public async Task<bool> RegisterAsync(string registrationCode, CancellationToken ct = default)
    {
        if (Device is null) Device = await _device.CollectAsync(Settings.CacheDirectory, ct);

        Connection = ConnectionState.Connecting;
        Notify();

        var result = await _coordinator.RegisterAsync(registrationCode, Settings.NodeName, Device, ct);

        if (!result.IsOk || result.Data is null)
        {
            Connection = ConnectionState.Offline;
            LastError = result.Message ?? "Qeydiyyat alınmadı";
            Notify();

            return false;
        }

        Settings.AccessToken = result.Data.AccessToken;
        Settings.NodeId = result.Data.NodeId;
        Settings.Save();

        _coordinator.SetToken(result.Data.AccessToken);
        Connection = ConnectionState.Online;
        LastError = null;
        Notify();

        Log.Info("worker", $"Cihaz qeydə alındı: #{result.Data.NodeId}");

        return true;
    }

    public Task StartAsync()
    {
        if (IsRunning) return Task.CompletedTask;

        _engineCancellation = new CancellationTokenSource();
        _loop = Task.Run(() => RunAsync(_engineCancellation.Token));

        Notify();
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_engineCancellation is null) return;

        await _engineCancellation.CancelAsync();

        if (_loop is not null)
        {
            try
            {
                await _loop;
            }
            catch (OperationCanceledException)
            {
                // Gözlənilən.
            }
        }

        _engineCancellation.Dispose();
        _engineCancellation = null;
        _loop = null;

        Connection = ConnectionState.Offline;
        Notify();
    }

    public void CancelJob(long jobId)
    {
        if (_cancellations.TryGetValue(jobId, out var source))
        {
            Log.Warn("worker", $"İş {jobId} ləğv edilir");
            source.Cancel();
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        Log.Info("worker", "Worker işə düşdü");

        if (!await PrepareAsync(ct)) return;

        if (!Settings.IsRegistered)
        {
            Connection = ConnectionState.Unauthorized;
            LastError = "Cihaz qeydiyyatdan keçməyib";
            Notify();
            return;
        }

        _coordinator.SetToken(Settings.AccessToken);

        while (!ct.IsCancellationRequested)
        {
            var delay = TimeSpan.FromSeconds(10);

            try
            {
                delay = await TickAsync(ct);
                _consecutiveFailures = 0;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _consecutiveFailures++;
                LastError = ex.Message;
                Log.Error("worker", $"Dövr xətası: {ex.Message}");

                // 10s → 20s → 40s … maksimum 5 dəqiqə.
                var backoff = TimeSpan.FromSeconds(Math.Min(10 * Math.Pow(2, _consecutiveFailures), 300));
                delay = backoff;

                Connection = ConnectionState.Offline;
                Notify();
            }

            try
            {
                await Task.Delay(delay, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        await StopJobsAsync();
        Connection = ConnectionState.Offline;
        Notify();
        Log.Info("worker", "Worker dayandı");
    }

    /// <summary>
    /// Profil həll olunubsa, serverə onun dəyərləri göndərilir — koordinator
    /// cihazın həqiqi tutumunu bilsin.
    /// </summary>
    private NodeRuntimeLimits EffectiveLimits()
    {
        var limits = Settings.Limits;

        if (Profile is null) return limits;

        return new NodeRuntimeLimits
        {
            Profile = Profile.Profile,
            MaxConcurrentJobs = Profile.MaxConcurrentJobs,
            MaxCpuThreads = Profile.CpuThreads,
            MaxCacheMb = limits.MaxCacheMb,
            WifiOnly = limits.WifiOnly,
            ChargingOnly = limits.ChargingOnly,
            PauseOnLowBattery = limits.PauseOnLowBattery,
            LowBatteryPercent = limits.LowBatteryPercent,
            PauseOnHighTemperature = limits.PauseOnHighTemperature,
            MaxTemperatureCelsius = limits.MaxTemperatureCelsius,
            ActiveHoursStart = limits.ActiveHoursStart,
            ActiveHoursEnd = limits.ActiveHoursEnd
        };
    }

    private async Task<TimeSpan> TickAsync(CancellationToken ct)
    {
        var limits = EffectiveLimits();

        PausedReason = EvaluateLimits(limits);
        Notify();

        var heartbeat = await _coordinator.HeartbeatAsync(new NodeHeartbeatRequest
        {
            Device = Device,
            Limits = limits,
            Benchmark = BenchmarkResult,
            Jobs = [.. _jobs.Values.Select(j => j.ToReport())]
        }, ct);

        if (heartbeat.Unauthorized)
        {
            // Token ləğv edilib və ya rotasiya olunub.
            Connection = ConnectionState.Unauthorized;
            LastError = "Token etibarsızdır — yenidən qeydiyyat lazımdır";
            Notify();

            await StopJobsAsync();
            return TimeSpan.FromMinutes(5);
        }

        if (!heartbeat.IsOk || heartbeat.Data is null)
        {
            Connection = ConnectionState.Offline;
            LastError = heartbeat.Message;
            Notify();

            _consecutiveFailures++;
            return TimeSpan.FromSeconds(Math.Min(10 * Math.Pow(2, _consecutiveFailures), 300));
        }

        Connection = ConnectionState.Online;
        LastError = null;

        // Koordinator bu işləri artıq tanımır (lease itdi və ya ləğv edildi).
        foreach (var jobId in heartbeat.Data.CancelJobIds)
        {
            CancelJob(jobId);
        }

        if (PausedReason is null)
        {
            await ClaimAsync(limits, ct);
        }

        Notify();

        return TimeSpan.FromSeconds(Math.Max(5, heartbeat.Data.PollSeconds));
    }

    private async Task ClaimAsync(NodeRuntimeLimits limits, CancellationToken ct)
    {
        var free = limits.MaxConcurrentJobs - _jobs.Count;
        if (free <= 0) return;

        var claimed = await _coordinator.ClaimAsync(free, ct);
        if (!claimed.IsOk || claimed.Data is null || claimed.Data.Count == 0) return;

        foreach (var assignment in claimed.Data)
        {
            var state = new RunningJob(assignment);
            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);

            _jobs[assignment.JobId] = state;
            _cancellations[assignment.JobId] = cancellation;

            Log.Info("worker", $"İş götürüldü: #{assignment.JobId} ({assignment.Resolution}, video {assignment.VideoId})");

            _ = Task.Run(() => ExecuteAsync(assignment, state, cancellation.Token), CancellationToken.None);
        }

        Notify();
    }

    private async Task ExecuteAsync(JobAssignment assignment, RunningJob state, CancellationToken ct)
    {
        var capabilities = Encoders ?? await _detector!.DetectAsync(Settings.PreferredEncoder, ct);

        try
        {
            var outcome = await new JobRunner(Settings, _coordinator, _storage, _encoder!, _probe!, Log, Profile)
                .RunAsync(assignment, capabilities, state, _coordinator.Token, ct);

            if (outcome.Success)
            {
                Log.Info("worker", $"İş {assignment.JobId} tamamlandı");
                Remember(new JobHistoryEntry(assignment.JobId, assignment.VideoId, assignment.Resolution,
                    true, null, DateTimeOffset.Now, (DateTimeOffset.Now - state.StartedAt).TotalSeconds));
                return;
            }

            Remember(new JobHistoryEntry(assignment.JobId, assignment.VideoId, assignment.Resolution,
                false, outcome.Error, DateTimeOffset.Now, (DateTimeOffset.Now - state.StartedAt).TotalSeconds));

            if (ct.IsCancellationRequested) return;

            Log.Warn("worker", $"İş {assignment.JobId} alınmadı: {outcome.Error}");

            await _coordinator.FailAsync(assignment.JobId, new JobFailRequest
            {
                AttemptToken = assignment.AttemptToken,
                Error = outcome.Error ?? "Naməlum xəta",
                Kind = outcome.Kind
            });
        }
        catch (LeaseLostException)
        {
            Log.Warn("worker", $"İş {assignment.JobId} artıq bu cihaza aid deyil — buraxılır");
            Remember(new JobHistoryEntry(assignment.JobId, assignment.VideoId, assignment.Resolution,
                false, "Lease itdi", DateTimeOffset.Now, (DateTimeOffset.Now - state.StartedAt).TotalSeconds));
        }
        catch (OperationCanceledException)
        {
            // Ləğv edildi (dayandırma və ya koordinator tələbi).
        }
        catch (Exception ex)
        {
            Log.Error("worker", $"İş {assignment.JobId} gözlənilməz xəta: {ex.Message}");

            try
            {
                await _coordinator.FailAsync(assignment.JobId, new JobFailRequest
                {
                    AttemptToken = assignment.AttemptToken,
                    Error = ex.Message,
                    Kind = JobErrorKind.Transient
                });
            }
            catch (Exception inner) when (inner is HttpRequestException or TaskCanceledException)
            {
                // Koordinator əlçatmazdır — lease bitəndə iş özü geri qaytarılacaq.
            }
        }
        finally
        {
            _jobs.TryRemove(assignment.JobId, out _);

            if (_cancellations.TryRemove(assignment.JobId, out var source)) source.Dispose();

            Notify();
        }
    }

    /// <summary>
    /// Resurs limitlərini yoxlayır. Ölçülə bilməyən göstəricilər (masaüstündə batareya)
    /// limiti bloklamır — yalnız həqiqətən bilinən dəyərlər tətbiq olunur.
    /// </summary>
    private string? EvaluateLimits(NodeRuntimeLimits limits)
    {
        if (limits.WifiOnly && !IsOnWifi()) return "Yalnız Wi-Fi gözlənilir";

        if (limits.ActiveHoursStart != limits.ActiveHoursEnd)
        {
            var hour = DateTimeOffset.Now.Hour;
            var active = limits.ActiveHoursStart < limits.ActiveHoursEnd
                ? hour >= limits.ActiveHoursStart && hour < limits.ActiveHoursEnd
                : hour >= limits.ActiveHoursStart || hour < limits.ActiveHoursEnd;

            if (!active) return $"Aktiv saatlar xaricində ({limits.ActiveHoursStart}:00–{limits.ActiveHoursEnd}:00)";
        }

        if (!_platform.SupportsPowerLimits) return null;

        var battery = _platform.BatteryPercent;
        var charging = _platform.IsCharging;

        if (limits.ChargingOnly && charging == false) return "Yalnız şarj olarkən işləyir";

        if (limits.PauseOnLowBattery && battery is { } percent && percent <= limits.LowBatteryPercent && charging != true)
        {
            return $"Batareya aşağıdır ({percent}%)";
        }

        if (limits.PauseOnHighTemperature && _platform.TemperatureCelsius is { } temperature && temperature >= limits.MaxTemperatureCelsius)
        {
            return $"Cihaz istidir ({temperature:0.#}°C)";
        }

        return null;
    }

    private static bool IsOnWifi()
        => NetworkInterface.GetAllNetworkInterfaces().Any(n =>
            n.OperationalStatus == OperationalStatus.Up
            && n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);

    private async Task StopJobsAsync()
    {
        foreach (var source in _cancellations.Values)
        {
            await source.CancelAsync();
        }

        // İşlərin öz təmizliyini bitirməsi üçün qısa fasilə.
        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);

        while (!_jobs.IsEmpty && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(100);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _http.Dispose();
    }
}
