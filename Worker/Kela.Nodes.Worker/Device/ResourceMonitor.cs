using System.Diagnostics;
using System.Globalization;

namespace Kela.Nodes.Worker.Device;

/// <summary>Bir anda ölçülmüş resurs göstəriciləri.</summary>
public sealed record ResourceSnapshot(
    double WorkerCpuPercent,
    double SystemCpuPercent,
    double? GpuPercent,
    double? GpuEncoderPercent,
    int RamUsedMb,
    int RamTotalMb,
    int DiskFreeMb)
{
    public static readonly ResourceSnapshot Empty = new(0, 0, null, null, 0, 0, 0);

    public double RamUsedPercent => RamTotalMb > 0 ? RamUsedMb * 100d / RamTotalMb : 0;
}

/// <summary>
/// CPU / GPU / RAM / disk göstəricilərini fon dövrəsində yığır. UI onları sadəcə
/// oxuyur — ölçmə işi encoding-ə mane olmasın deyə ayrıca axında və seyrək aparılır.
/// </summary>
public sealed class ResourceMonitor(string cacheDirectory, WorkerLog log)
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(3);

    private ResourceSnapshot _current = ResourceSnapshot.Empty;
    private Task? _loop;
    private CancellationTokenSource? _cancellation;

    private long _lastWorkerTicks;
    private double _lastWorkerSample;
    private long _lastTotalTicks;
    private long _lastIdleTicks;

    /// <summary>Android də Linux nüvəsi işlədir, amma IsLinux() orada false qaytarır.</summary>
    private static bool IsUnix => OperatingSystem.IsLinux() || OperatingSystem.IsAndroid();

    public ResourceSnapshot Current => _current;

    public bool GpuAvailable { get; private set; }

    public void Start(CancellationToken ct)
    {
        if (_loop is { IsCompleted: false }) return;

        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loop = Task.Run(() => LoopAsync(_cancellation.Token));
    }

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                _current = await SampleAsync(ct);
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
            {
                log.Debug("resources", $"Ölçmə alınmadı: {ex.Message}");
            }

            try
            {
                await Task.Delay(Interval, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task<ResourceSnapshot> SampleAsync(CancellationToken ct)
    {
        var now = Stopwatch.GetTimestamp();
        var (systemCpu, total, idle) = SystemCpu();
        var workerCpu = WorkerCpu(now);

        _lastTotalTicks = total;
        _lastIdleTicks = idle;
        _lastWorkerTicks = Process.GetCurrentProcess().TotalProcessorTime.Ticks;
        _lastWorkerSample = now;

        var (gpu, encoder) = await GpuAsync(ct);

        var (totalRam, availableRam) = Ram();

        return new ResourceSnapshot(
            Math.Round(workerCpu, 1),
            Math.Round(systemCpu, 1),
            gpu,
            encoder,
            Math.Max(0, totalRam - availableRam),
            totalRam,
            SystemPressure.FreeDiskMb(cacheDirectory));
    }

    /// <summary>İşçi prosesin öz CPU payı — bütün nüvələrə nisbətdə faiz.</summary>
    private double WorkerCpu(double now)
    {
        if (_lastWorkerSample <= 0) return 0;

        var elapsed = Stopwatch.GetElapsedTime((long)_lastWorkerSample, (long)now).TotalSeconds;
        if (elapsed <= 0) return 0;

        var used = TimeSpan.FromTicks(Process.GetCurrentProcess().TotalProcessorTime.Ticks - _lastWorkerTicks);

        return used.TotalSeconds / (elapsed * Environment.ProcessorCount) * 100;
    }

    /// <summary>Sistemin ümumi CPU yükü (/proc/stat). Digər platformalarda 0 qaytarır.</summary>
    private (double Percent, long Total, long Idle) SystemCpu()
    {
        if (!IsUnix || _lastTotalTicks == 0)
        {
            if (!IsUnix) return (0, 0, 0);

            var (t0, i0) = ReadProcStat();
            return (0, t0, i0);
        }

        var (total, idle) = ReadProcStat();

        var totalDelta = total - _lastTotalTicks;
        var idleDelta = idle - _lastIdleTicks;

        if (totalDelta <= 0) return (0, total, idle);

        return ((totalDelta - idleDelta) * 100d / totalDelta, total, idle);
    }

    private static (long Total, long Idle) ReadProcStat()
    {
        try
        {
            var line = File.ReadLines("/proc/stat").FirstOrDefault(l => l.StartsWith("cpu ", StringComparison.Ordinal));
            if (line is null) return (0, 0);

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1)
                .Select(p => long.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0)
                .ToArray();

            if (parts.Length < 4) return (0, 0);

            // idle + iowait boş sayılır.
            return (parts.Sum(), parts[3] + (parts.Length > 4 ? parts[4] : 0));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return (0, 0);
        }
    }

    private static (int TotalMb, int AvailableMb) Ram()
    {
        try
        {
            if (IsUnix)
            {
                long totalKb = 0, availableKb = 0;

                foreach (var line in File.ReadLines("/proc/meminfo"))
                {
                    if (line.StartsWith("MemTotal:", StringComparison.Ordinal)) totalKb = ToKb(line);
                    else if (line.StartsWith("MemAvailable:", StringComparison.Ordinal)) availableKb = ToKb(line);
                }

                if (totalKb > 0) return ((int)(totalKb / 1024), (int)(availableKb / 1024));
            }

            var fallback = (int)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024);
            return (fallback, fallback);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return (0, 0);
        }
    }

    private static long ToKb(string line)
    {
        var value = line[(line.IndexOf(':') + 1)..].Replace("kB", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var kb) ? kb : 0;
    }

    /// <summary>GPU istifadəsi. nvidia-smi yoxdursa null qaytarır — uydurma dəyər göstərilmir.</summary>
    private async Task<(double? Gpu, double? Encoder)> GpuAsync(CancellationToken ct)
    {
        try
        {
            var result = await Encoding.ProcessRunner.RunAsync("nvidia-smi",
                ["--query-gpu=utilization.gpu,utilization.encoder", "--format=csv,noheader,nounits"],
                timeout: TimeSpan.FromSeconds(5), ct: ct);

            if (!result.Success) return (null, null);

            var first = result.Output
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (first is null) return (null, null);

            var parts = first.Split(',', StringSplitOptions.TrimEntries);

            GpuAvailable = true;

            return (
                parts.Length > 0 && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var gpu) ? gpu : null,
                parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var enc) ? enc : null);
        }
        catch (Exception ex) when (ex is IOException or System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return (null, null);
        }
    }
}
