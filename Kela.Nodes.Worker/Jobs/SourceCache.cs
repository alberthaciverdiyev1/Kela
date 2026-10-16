using System.Collections.Concurrent;

namespace Kela.Nodes.Worker.Jobs;

/// <summary>
/// Eyni videonun mənbə faylını bir dəfə endirir və bütün pillələr onu paylaşır.
///
/// Əvvəl hər tapşırıq öz nüsxəsini endirirdi: eyni video üçün 4 pillə paralel
/// başlayanda 4 dəfə eyni fayl şəbəkədən çəkilirdi. İndi birinci endirməni başladır,
/// qalanlar həmin faylı gözləyir — yəni paralellik pozulmur, sadəcə təkrar
/// endirmə olmur.
/// </summary>
public sealed class SourceCache(WorkerStorage storage, WorkerSettings settings, WorkerLog log)
{
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _locks = new();

    /// <summary>Keşlənmiş mənbənin yolu; endirmə mümkün olmasa null.</summary>
    public async Task<string?> AcquireAsync(
        long videoId, string sourceKey, string sourceUrl, IProgress<TransferProgress>? progress, CancellationToken ct)
    {
        var extension = Path.GetExtension(sourceKey);
        if (string.IsNullOrWhiteSpace(extension)) extension = ".mp4";

        var directory = Path.Combine(settings.CacheDirectory, "sources");
        var cached = Path.Combine(directory, videoId + extension);

        if (File.Exists(cached) && new FileInfo(cached).Length > 0)
        {
            log.Debug("cache", $"Mənbə keşdən: video {videoId}");
            return cached;
        }

        var gate = _locks.GetOrAdd(videoId, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync(ct);

        try
        {
            // Kilidi gözləyərkən başqa pillə endirib bitirmiş ola bilər.
            if (File.Exists(cached) && new FileInfo(cached).Length > 0) return cached;

            Directory.CreateDirectory(directory);
            var partial = cached + ".part";

            await storage.DownloadAsync(sourceUrl, partial, progress, ct);

            File.Move(partial, cached, overwrite: true);
            log.Info("cache", $"Mənbə endirildi: video {videoId} ({new FileInfo(cached).Length / 1024 / 1024} MB)");

            return cached;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException)
        {
            log.Warn("cache", $"Mənbə endirilə bilmədi (video {videoId}): {ex.Message}");

            TryDelete(cached + ".part");
            return null;
        }
        finally
        {
            gate.Release();

            if (gate.CurrentCount == 1) _locks.TryRemove(videoId, out _);
        }
    }

    /// <summary>Keş həddi aşıbsa ən köhnə mənbələri silir.</summary>
    public void Trim()
    {
        var limitMb = settings.Limits.MaxCacheMb;
        if (limitMb <= 0) return;

        try
        {
            var directory = Path.Combine(settings.CacheDirectory, "sources");
            if (!Directory.Exists(directory)) return;

            var files = new DirectoryInfo(directory).GetFiles()
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .ToList();

            long total = files.Sum(f => f.Length);
            var limitBytes = (long)limitMb * 1024 * 1024;

            foreach (var file in files)
            {
                if (total <= limitBytes) break;

                total -= file.Length;

                // Hazırda oxunan fayl silinə bilməz — həmin iş uğursuz olardı.
                try
                {
                    file.Delete();
                    log.Debug("cache", $"Keşdən silindi: {file.Name}");
                }
                catch (IOException)
                {
                    // Fayl məşğuldur — növbəti dəfə yoxlanılacaq.
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Keş təmizliyi alınmasa konversiya dayanmır.
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (IOException)
        {
            // Yarımçıq fayl qala bilər.
        }
    }
}
