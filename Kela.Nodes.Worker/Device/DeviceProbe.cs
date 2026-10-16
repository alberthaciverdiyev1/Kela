using System.Globalization;
using System.Runtime.InteropServices;
using Kela.Nodes.Shared.Contracts;

namespace Kela.Nodes.Worker.Device;

/// <summary>
/// Batareya və temperatur kimi yalnız bəzi platformalarda olan göstəriciləri verir.
/// Desktop tətbiqi boş implementasiya işlədir; Android öz native versiyasını verir.
/// </summary>
public interface IPlatformStatus
{
    int? BatteryPercent { get; }

    bool? IsCharging { get; }

    double? TemperatureCelsius { get; }

    /// <summary>Ölçmə mümkün deyilsə true — limitlər tətbiq olunmamalıdır.</summary>
    bool SupportsPowerLimits { get; }
}

public sealed class NullPlatformStatus : IPlatformStatus
{
    public int? BatteryPercent => null;

    public bool? IsCharging => null;

    public double? TemperatureCelsius => null;

    public bool SupportsPowerLimits => false;
}

/// <summary>Cihazın aşkarlanmış xüsusiyyətlərini toplayır.</summary>
public sealed class DeviceProbe(IPlatformStatus platform, WorkerLog log)
{
    public async Task<NodeDeviceInfo> CollectAsync(string cacheDirectory, CancellationToken ct = default)
    {
        var info = new NodeDeviceInfo
        {
            OperatingSystem = OperatingSystemName(),
            OsVersion = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(),
            MachineName = Environment.MachineName,
            CpuCores = Environment.ProcessorCount,
            CpuModel = await CpuModelAsync(ct),
            Gpus = await GpusAsync(ct),
            BatteryPercent = platform.BatteryPercent,
            Charging = platform.IsCharging,
            TemperatureCelsius = platform.TemperatureCelsius
        };

        var (totalRam, availableRam) = Memory();
        info.TotalRamMb = totalRam;
        info.AvailableRamMb = availableRam;

        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(cacheDirectory)) ?? "/");
            info.DiskFreeMb = (int)Math.Min(int.MaxValue, drive.AvailableFreeSpace / (1024 * 1024));
            info.DiskTotalMb = (int)Math.Min(int.MaxValue, drive.TotalSize / (1024 * 1024));
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException)
        {
            log.Warn("device", "Disk məlumatı oxunmadı");
        }

        return info;
    }

    private static string OperatingSystemName()
    {
        if (OperatingSystem.IsAndroid()) return "Android";
        if (OperatingSystem.IsIOS()) return "iOS";
        if (OperatingSystem.IsWindows()) return "Windows";
        if (OperatingSystem.IsMacOS()) return "macOS";
        if (OperatingSystem.IsLinux()) return "Linux";
        if (OperatingSystem.IsFreeBSD()) return "FreeBSD";

        return "Unknown";
    }

    private static async Task<string?> CpuModelAsync(CancellationToken ct)
    {
        try
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsAndroid())
            {
                foreach (var line in await File.ReadAllLinesAsync("/proc/cpuinfo", ct))
                {
                    if (line.StartsWith("model name", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2) return parts[1].Trim();
                    }
                }

                return null;
            }

            if (OperatingSystem.IsMacOS())
            {
                return await SysctlAsync("machdep.cpu.brand_string", ct);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // CPU adı vacib deyil — qalan məlumat kifayətdir.
        }

        return null;
    }

    private static async Task<string?> SysctlAsync(string key, CancellationToken ct)
    {
        var result = await Encoding.ProcessRunner.RunAsync("/usr/sbin/sysctl", ["-n", key], timeout: TimeSpan.FromSeconds(5), ct: ct);

        return result.Success ? result.Output.Trim() : null;
    }

    /// <summary>
    /// GPU adları ən yaxşı səylə toplanır. Əsl həlledici məlumat ffmpeg-in təsdiqlənmiş
    /// encoder siyahısıdır — buradakı ad yalnız panelə məlumat üçündür.
    /// </summary>
    private async Task<List<string>> GpusAsync(CancellationToken ct)
    {
        var gpus = new List<string>();

        try
        {
            var nvidia = await Encoding.ProcessRunner.RunAsync("nvidia-smi",
                ["--query-gpu=name", "--format=csv,noheader"], timeout: TimeSpan.FromSeconds(8), ct: ct);

            if (nvidia.Success)
            {
                gpus.AddRange(nvidia.Output
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => line.Length > 0));
            }
        }
        catch (Exception ex) when (ex is IOException or System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // nvidia-smi yoxdur.
        }

        if (gpus.Count == 0 && OperatingSystem.IsLinux())
        {
            try
            {
                gpus.AddRange(LinuxDrmDevices());
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                // Bəzi sistemlərdə /sys/class/drm mövcuddur, amma oxunmur — bu, ölümcül deyil.
                log.Debug("device", "DRM məlumatı oxunmadı");
            }
        }

        return gpus.Distinct().ToList();
    }

    private static IEnumerable<string> LinuxDrmDevices()
    {
        const string root = "/sys/class/drm";

        if (!Directory.Exists(root)) yield break;

        foreach (var card in Directory.EnumerateDirectories(root, "card[0-9]*"))
        {
            var device = Path.Combine(card, "device");

            if (!Directory.Exists(device)) continue;

            var vendor = ReadFirstLine(Path.Combine(device, "vendor"));
            var name = ReadFirstLine(Path.Combine(device, "uevent")) ?? string.Empty;

            var label = vendor switch
            {
                "0x10de" => "NVIDIA",
                "0x8086" => "Intel",
                "0x1002" or "0x1022" => "AMD",
                _ => null
            };

            if (label is not null && !name.Contains("DRIVER=", StringComparison.Ordinal)) continue;
            if (label is not null) yield return label;
        }
    }

    private static string? ReadFirstLine(string file)
    {
        try
        {
            return File.Exists(file) ? File.ReadLines(file).FirstOrDefault()?.Trim() : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static (int TotalMb, int AvailableMb) Memory()
    {
        try
        {
            var total = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

            if (OperatingSystem.IsLinux() || OperatingSystem.IsAndroid())
            {
                var lines = File.ReadAllLines("/proc/meminfo");
                var totalKb = ReadMemValue(lines, "MemTotal:");
                var availableKb = ReadMemValue(lines, "MemAvailable:");

                if (totalKb > 0) total = totalKb * 1024;
                if (availableKb > 0) return (Mb(total), Mb(availableKb * 1024));
            }

            return (Mb(total), Mb(total));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return (0, 0);
        }
    }

    private static long ReadMemValue(string[] lines, string key)
    {
        foreach (var line in lines)
        {
            if (!line.StartsWith(key, StringComparison.Ordinal)) continue;

            var value = line[key.Length..].Replace("kB", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        }

        return 0;
    }

    private static int Mb(long bytes) => (int)Math.Clamp(bytes / (1024 * 1024), 0, int.MaxValue);
}
