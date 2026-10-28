using System.Net.Http.Headers;

namespace Edutor.Nodes.Worker;

/// <summary>
/// Mənbə storage-da yoxdur (404). Bu, keçici deyil — təkrar cəhd heç nəyi
/// dəyişməz, ona görə tapşırıq dərhal qalıcı uğursuz sayılır.
/// </summary>
public sealed class SourceUnavailableException(string message) : Exception(message);

/// <summary>Endirmə gedişatı: nə qədər gəldi, nə qədər qalıb, hansı sürətlə.</summary>
public sealed record TransferProgress(long Bytes, long TotalBytes, double BytesPerSecond)
{
    public int Percent => TotalBytes > 0 ? (int)Math.Clamp(Bytes * 100 / TotalBytes, 0, 100) : 0;

    public double Megabytes => Bytes / 1024d / 1024d;

    public double TotalMegabytes => TotalBytes / 1024d / 1024d;

    public double MegabytesPerSecond => BytesPerSecond / 1024d / 1024d;
}

/// <summary>Mənbəni endirir və çıxışları imzalanmış URL-lərə yükləyir.</summary>
public sealed class WorkerStorage(HttpClient http, WorkerLog log)
{
    /// <summary>
    /// Transferdə bu müddət ərzində heç bir bayt gəlməzsə, əlaqə donmuş sayılır və
    /// kəsilir. Ümumi timeout qoymaq olmaz — 5 GB-lıq fayl da qanuni ola bilər;
    /// ölçü deyil, <b>hərəkətsizlik</b> problemdir.
    /// </summary>
    private static readonly TimeSpan StallTimeout = TimeSpan.FromSeconds(60);

    /// <summary>Bir seqment/playlist üçün ümumi yükləmə müddəti — kiçik fayllardır.</summary>
    private static readonly TimeSpan UploadTimeout = TimeSpan.FromMinutes(10);

    /// <summary>Endirmə — çıxış axını ilə, yaddaşa yığmadan.</summary>
    public async Task DownloadAsync(string url, string destination, IProgress<TransferProgress>? progress = null, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new SourceUnavailableException("Mənbə fayl storage-da tapılmadı");
        }

        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1;
        var buffer = new byte[81920];
        long written = 0;
        var lastReport = 0L;
        var clock = System.Diagnostics.Stopwatch.StartNew();

        await using var source = await response.Content.ReadAsStreamAsync(ct);
        await using var target = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, useAsync: true);

        while (true)
        {
            // Hər oxuma üçün ayrıca geri sayım: bayt gəldikcə sıfırlanır, susanda kəsilir.
            using var stall = CancellationTokenSource.CreateLinkedTokenSource(ct);
            stall.CancelAfter(StallTimeout);

            int read;

            try
            {
                read = await source.ReadAsync(buffer, stall.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new HttpRequestException($"Endirmə {StallTimeout.TotalSeconds:0} saniyə hərəkətsiz qaldı");
            }

            if (read <= 0) break;

            await target.WriteAsync(buffer.AsMemory(0, read), ct);
            written += read;

            // Hər 4 MB-dan bir bildiriş — UI-ı yormamaq üçün.
            if (progress is not null && written - lastReport >= 4 * 1024 * 1024)
            {
                lastReport = written;
                var seconds = clock.Elapsed.TotalSeconds;
                progress.Report(new TransferProgress(written, total, seconds > 0 ? written / seconds : 0));
            }
        }

        clock.Stop();
        progress?.Report(new TransferProgress(written, total, clock.Elapsed.TotalSeconds > 0
            ? written / clock.Elapsed.TotalSeconds
            : 0));
        log.Debug("storage", $"Endirildi: {Path.GetFileName(destination)} ({written / 1024 / 1024} MB)");
    }

    /// <summary>
    /// Faylı imzalanmış PUT URL-inə göndərir. Presigned URL üçün Content-Type imzalanarkən
    /// istifadə olunan dəyərlə eyni olmalıdır.
    /// </summary>
    /// <param name="authenticated">
    /// true = URL API-nin öz proxy-sidir (presigned PUT dəstəklənməyəndə) və cihaz
    /// tokeni tələb edir. Presigned URL-lərə başlıq əlavə olunmur.
    /// </param>
    public async Task UploadAsync(string url, string file, string contentType, bool authenticated,
        string? token = null, CancellationToken ct = default)
    {
        await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = content };

        if (authenticated && !string.IsNullOrEmpty(token))
        {
            request.Headers.Add("X-Node-Token", token);
        }

        // Yükləmə də dona bilər — cavab gözləmə müddəti məhdudlaşdırılır.
        using var stall = CancellationTokenSource.CreateLinkedTokenSource(ct);
        stall.CancelAfter(UploadTimeout);

        using var response = await http.SendAsync(request, stall.Token);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Upload failed ({(int)response.StatusCode}) for {Path.GetFileName(file)}: {Trim(body)}");
        }
    }

    private static string Trim(string value) => value.Length <= 200 ? value : value[..200];
}
