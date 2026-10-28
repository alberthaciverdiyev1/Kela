using System.Globalization;

namespace Edutor.Nodes.Worker.Device;

/// <summary>Cari boş yaddaş və disk — paralellik qərarı üçün.</summary>
public static class SystemPressure
{
    public static int AvailableRamMb()
    {
        try
        {
            // Android Linux nüvəsidir — IsLinux() orada false qaytarır, ona görə ayrıca yoxlanılır.
            if (OperatingSystem.IsLinux() || OperatingSystem.IsAndroid())
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
            // Qovluq hələ yoxdursa yaradılır ki, onun diskini ölçə bilək.
            Directory.CreateDirectory(cacheDirectory);

            var root = Path.GetPathRoot(Path.GetFullPath(cacheDirectory));
            if (string.IsNullOrEmpty(root)) return 0;

            return (int)Math.Clamp(new DriveInfo(root).AvailableFreeSpace / (1024 * 1024), 0, int.MaxValue);
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException)
        {
            return 0;
        }
    }
}
