using System.Diagnostics;

namespace Kela.Api.Media;

/// <summary>
/// Video dosyalarını diske kaydeder, ffprobe ile süreyi okur ve
/// ffmpeg ile videodan otomatik thumbnail (1. saniyedeki kare) çıkarır.
/// </summary>
public sealed class MediaProcessor
{
    private readonly string _uploadsRoot;

    public MediaProcessor(IWebHostEnvironment env, IConfiguration config)
    {
        _uploadsRoot = Path.GetFullPath(
            config["Uploads:Root"] ?? Path.Combine(env.ContentRootPath, "uploads"));
    }

    public string UploadsRoot => _uploadsRoot;
    public string VideosDir => Path.Combine(_uploadsRoot, "videos");
    public string ThumbnailsDir => Path.Combine(_uploadsRoot, "thumbnails");

    /// <summary>Videoyu diske yazar, süresini ve thumbnail'ini üretir.</summary>
    /// <returns>(videoUrlPath, thumbnailUrlPath, durationSeconds)</returns>
    public (string VideoPath, string ThumbnailPath, int DurationSeconds) SaveVideo(Stream stream, string originalName)
    {
        Directory.CreateDirectory(VideosDir);
        Directory.CreateDirectory(ThumbnailsDir);

        var ext = Path.GetExtension(originalName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext) || !IsVideoExtension(ext))
        {
            ext = ".mp4";
        }

        var id = Guid.NewGuid().ToString("N");
        var videoName = id + ext;
        var thumbName = id + ".jpg";

        var videoFile = Path.Combine(VideosDir, videoName);
        var thumbFile = Path.Combine(ThumbnailsDir, thumbName);

        using (var file = new FileStream(videoFile, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024))
        {
            stream.CopyTo(file);
        }

        var duration = ProbeDuration(videoFile);
        ExtractThumbnail(videoFile, thumbFile);

        return ($"/uploads/videos/{videoName}", $"/uploads/thumbnails/{thumbName}", duration);
    }

    /// <summary>DB'deki URL yolu (<c>/uploads/...</c>) → fiziksel dosya yolu.</summary>
    public string? ResolvePhysicalPath(string urlPath)
    {
        if (string.IsNullOrWhiteSpace(urlPath))
        {
            return null;
        }

        // DB'deki yol "/uploads/videos/x.mp4" biçimindedir; _uploadsRoot zaten "uploads" köküdür,
        // bu nedenle öndeki "/uploads/" parçasını ayıkla (yoksa çift önek oluşur).
        var relative = urlPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        if (relative.StartsWith($"uploads{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
        {
            relative = relative[($"uploads{Path.DirectorySeparatorChar}").Length..];
        }

        var full = Path.GetFullPath(Path.Combine(_uploadsRoot, relative));
        var root = Path.GetFullPath(_uploadsRoot) + Path.DirectorySeparatorChar;
        return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(full) ? full : null;
    }

    public static string ContentTypeFor(string urlPath)
    {
        var ext = Path.GetExtension(urlPath).ToLowerInvariant();
        return ext switch
        {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogg" => "video/ogg",
            ".mov" => "video/quicktime",
            ".m4v" => "video/x-m4v",
            ".mkv" => "video/x-matroska",
            ".avi" => "video/x-msvideo",
            _ => "application/octet-stream",
        };
    }

    private static bool IsVideoExtension(string ext) => ext is
        ".mp4" or ".webm" or ".ogg" or ".mov" or ".m4v" or ".mkv" or ".avi" or ".mpg" or ".mpeg";

    /// <summary>ffprobe ile süre (saniye, aşağı yuvarlanır).</summary>
    private static int ProbeDuration(string videoFile)
    {
        var psi = new ProcessStartInfo("ffprobe")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            ArgumentList = { "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", videoFile },
        };

        using var proc = Process.Start(psi);
        if (proc is null)
        {
            return 0;
        }

        var output = proc.StandardOutput.ReadToEnd().Trim();
        proc.WaitForExit(30_000);

        return double.TryParse(output, System.Globalization.CultureInfo.InvariantCulture, out var seconds)
            ? (int)Math.Round(seconds)
            : 0;
    }

    /// <summary>ffmpeg ile videonun 1. saniyesindeki kareyi thumbnail olarak kaydeder.</summary>
    private static void ExtractThumbnail(string videoFile, string thumbFile)
    {
        var psi = new ProcessStartInfo("ffmpeg")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            ArgumentList =
            {
                "-i", videoFile,
                "-ss", "00:00:01",
                "-vframes", "1",
                "-vf", "scale=640:-2",
                "-y", thumbFile,
            },
        };

        using var proc = Process.Start(psi);
        if (proc is null)
        {
            return;
        }

        // ffmpeg progress stderr'e yazar; ReadToEnd okuyana kadar çıkmayabilir.
        _ = proc.StandardError.ReadToEndAsync();
        if (!proc.WaitForExit(60_000))
        {
            try { proc.Kill(entireProcessTree: true); } catch { /* boş ver */ }
        }
    }
}
