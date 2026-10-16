using System.Globalization;

namespace Kela.Nodes.Worker.Encoding;

public sealed record EncodeProgress(double Fraction, double? Fps, double? Speed);

public sealed record EncodeOutcome(bool Success, string? Error);

/// <summary>Bir HLS variantını kodlaşdırır və ffmpeg-in irəliləyişini oxuyur.</summary>
public sealed class HlsEncoder(string ffmpegPath, WorkerLog log)
{
    public async Task<EncodeOutcome> EncodeAsync(
        EncodeRequest request,
        IProgress<EncodeProgress>? progress = null,
        bool belowNormalPriority = false,
        CancellationToken ct = default)
    {
        Directory.CreateDirectory(request.OutputDirectory);

        var arguments = EncodePlan.Hls(request);
        log.Debug("ffmpeg", $"{request.Profile.Name}: ffmpeg {string.Join(' ', arguments)}");

        var duration = request.DurationSeconds;
        var errors = new List<string>();
        var lastError = string.Empty;

        var onLine = (string line) =>
        {
            if (line.StartsWith("out_time_us=", StringComparison.Ordinal))
            {
                if (duration > 0 && double.TryParse(line[12..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var micros))
                {
                    progress?.Report(new EncodeProgress(Math.Clamp(micros / 1_000_000d / duration, 0, 1), null, null));
                }

                return;
            }

            if (line.StartsWith("fps=", StringComparison.Ordinal))
            {
                if (double.TryParse(line[4..], NumberStyles.Float, CultureInfo.InvariantCulture, out var fps) && fps > 0)
                {
                    progress?.Report(new EncodeProgress(-1, fps, null));
                }

                return;
            }

            if (line.StartsWith("speed=", StringComparison.Ordinal))
            {
                var value = line[6..].TrimEnd('x', ' ');
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var speed) && speed > 0)
                {
                    progress?.Report(new EncodeProgress(-1, null, speed));
                }

                return;
            }

            if (line.Length > 0 && errors.Count < 40)
            {
                errors.Add(line);
                lastError = line;
            }
        };

        var result = await ProcessRunner.RunAsync(ffmpegPath, arguments, onLine, timeout: null,
            belowNormalPriority: belowNormalPriority, ct: ct);

        if (result.Success) return new EncodeOutcome(true, null);

        var detail = lastError.Length > 0 ? lastError : result.Error.Trim();
        if (detail.Length > 500) detail = detail[^500..];

        return new EncodeOutcome(false, $"ffmpeg exit {result.ExitCode}: {detail}");
    }

    public async Task<bool> CreateThumbnailAsync(string input, string output, int videoStreamIndex, int durationSeconds, CancellationToken ct = default)
    {
        // Videonun 10%-dən (çox qısadırsa 1-ci saniyədən) kadr götürülür.
        var at = durationSeconds > 10 ? durationSeconds * 0.1 : 1;

        var result = await ProcessRunner.RunAsync(ffmpegPath,
            EncodePlan.Thumbnail(input, output, videoStreamIndex, at),
            timeout: TimeSpan.FromMinutes(2), ct: ct);

        if (result.Success) return true;

        // Bəzi fayllarda həmin saniyə yoxdur — başlanğıcdan yoxlanılır.
        var fallback = await ProcessRunner.RunAsync(ffmpegPath,
            EncodePlan.Thumbnail(input, output, videoStreamIndex, 0),
            timeout: TimeSpan.FromMinutes(2), ct: ct);

        if (!fallback.Success) log.Warn("ffmpeg", "Thumbnail yaradıla bilmədi");

        return fallback.Success;
    }
}
