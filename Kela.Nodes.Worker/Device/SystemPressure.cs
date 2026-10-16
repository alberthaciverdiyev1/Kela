using System.Globalization;

namespace Kela.Nodes.Worker.Device;

/// <summary>Cari boş yaddaş və disk — paralellik qərarı üçün.</summary>
public static class SystemPressure
{
    private const string CacheProbeDirectory = "cache";

    public static int AvailableRamMb()
    {
        try
        {
            if (OperatingSystem.IsLinux())
            {
                foreach (var line in File.ReadLines("/proc/meminfo"))
                {
                    if (!line.StartsWith("MemAvailable:", StringComparison.Ordinal)) continue;

                    var value = line["MemAvailable:".Length..]
                        .Replace("kB", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

                    if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var kb))
                    {
                        return (int)Math.Clamp(kb / 1024, 0, int.MaxValue);
                    }
                }

                return 0;
            }

            return (int)Math.Clamp(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024), 0, int.MaxValue);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return 0;
        }
    }

    public static int FreeDiskMb(string cacheDirectory)
    {
        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(cacheDirectory));

            // Qovluq hələ yaradılmayıbsa valideyninə baxılır.
            while (!string.IsNullOrEmpty(root) && !Directory.Exists(root))
            {
                root = Path.GetPathRoot(Path.GetDirectoryName(root.TrimEnd(Path.DirectorySeparatorChar)) ?? root);
            }

            if (string.IsNullOrEmpty(root)) return 0;

            return (int)Math.Clamp(new DriveInfo(root).AvailableFreeSpace / (1024 * 1024), 0, int.MaxValue);
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException)
        {
            return 0;
        }
    }

    public static string CacheDirectory(string workerCacheDirectory)
        => Path.Combine(workerCacheDirectory, CacheProbeDirectory);
}
