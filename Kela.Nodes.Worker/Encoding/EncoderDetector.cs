namespace Kela.Nodes.Worker.Encoding;

public sealed record EncoderChoice(string Name, string HwAccel, bool Hardware);

public sealed record EncoderCapabilities(
    EncoderChoice Chosen,
    IReadOnlyList<EncoderChoice> Available,
    IReadOnlyList<string> HardwareAcceleration)
{
    public IReadOnlyList<string> Names => [.. Available.Select(a => a.Name)];
}

/// <summary>
/// Mövcud H.264 encoder-ləri tapır və <b>hər birini real kodlaşdırma ilə yoxlayır</b>.
/// Cihazda NVIDIA kartı olması encoder-in işlədiyi demək deyil — sürücü, konteyner
/// icazələri və ya vaapi cihazı olmaya bilər, ona görə sınaq vacibdir.
/// </summary>
public sealed class EncoderDetector(string ffmpegPath, WorkerLog log)
{
    private sealed record Candidate(string Name, string HwAccel, bool Hardware);

    private static readonly Candidate[] Candidates =
    [
        new("h264_nvenc", "cuda", true),
        new("h264_qsv", "qsv", true),
        new("h264_videotoolbox", "videotoolbox", true),
        new("h264_amf", "amf", true),
        new("h264_mediacodec", "mediacodec", true),
        new("h264_vaapi", "vaapi", true),
        new("libx264", "cpu", false)
    ];

    public async Task<EncoderCapabilities> DetectAsync(string? preferred, CancellationToken ct = default)
    {
        var accelerated = await HardwareAccelerationAsync(ct);
        var available = new List<EncoderChoice>();

        foreach (var candidate in Candidates)
        {
            // Bir aparat encoder-i kifayətdir — qalan aparat namizədləri eyni işi görür.
            // CPU encoder-i isə ayrıca yoxlanılır: hibrid rejimdə ona da ehtiyac var.
            if (candidate.Hardware && available.Any(a => a.Hardware))
            {
                continue;
            }

            if (candidate.Hardware && !await ListedAsync(candidate.Name, ct))
            {
                log.Debug("encoder", $"{candidate.Name} bu ffmpeg quruluşunda yoxdur");
                continue;
            }

            if (await VerifyAsync(candidate, ct))
            {
                available.Add(new EncoderChoice(candidate.Name, candidate.HwAccel, candidate.Hardware));
                log.Info("encoder", $"{candidate.Name} təsdiqləndi ({(candidate.Hardware ? "apparat" : "CPU")})");
            }
            else if (candidate.Hardware)
            {
                log.Warn("encoder", $"{candidate.Name} mövcuddur, amma sınaq kodlaşdırması alınmadı — istifadə olunmayacaq");
            }
        }

        if (available.Count == 0)
        {
            throw new InvalidOperationException("Heç bir H.264 encoder işləmir — ffmpeg quruluşunu yoxlayın");
        }

        var chosen = !string.IsNullOrWhiteSpace(preferred)
            ? available.FirstOrDefault(a => string.Equals(a.Name, preferred, StringComparison.OrdinalIgnoreCase))
            : null;

        chosen ??= available[0];

        if (!string.IsNullOrWhiteSpace(preferred) && !string.Equals(chosen.Name, preferred, StringComparison.OrdinalIgnoreCase))
        {
            log.Warn("encoder", $"Seçilmiş encoder '{preferred}' işləmir — '{chosen.Name}' istifadə olunur");
        }

        return new EncoderCapabilities(chosen, available, accelerated);
    }

    /// <summary>NVENC üçün istifadə oluna biləcək NVIDIA kartlarının sayı.</summary>
    public async Task<int> GpuCountAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await ProcessRunner.RunAsync("nvidia-smi", ["--list-gpus"], timeout: TimeSpan.FromSeconds(10), ct: ct);

            if (!result.Success) return 1;

            var count = result.Output
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Count(line => line.StartsWith("GPU ", StringComparison.Ordinal));

            return Math.Max(1, count);
        }
        catch (Exception ex) when (ex is IOException or System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // nvidia-smi yoxdur — NVENC də seçilməyəcək, amma CPU zolaqları qalır.
            return 1;
        }
    }

    private async Task<IReadOnlyList<string>> HardwareAccelerationAsync(CancellationToken ct)
    {
        var result = await ProcessRunner.RunAsync(ffmpegPath, ["-hide_banner", "-hwaccels"], timeout: TimeSpan.FromSeconds(20), ct: ct);
        if (!result.Success) return [];

        return [.. result.Output
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith("Hardware", StringComparison.OrdinalIgnoreCase))
            .Distinct()];
    }

    private async Task<bool> ListedAsync(string encoder, CancellationToken ct)
    {
        var result = await ProcessRunner.RunAsync(ffmpegPath, ["-hide_banner", "-encoders"], timeout: TimeSpan.FromSeconds(20), ct: ct);

        return result.Success && result.Output.Contains(encoder, StringComparison.Ordinal);
    }

    /// <summary>Qısa sintetik klip kodlaşdırılır — encoder həqiqətən işləyirsə exit code 0 olur.</summary>
    private async Task<bool> VerifyAsync(Candidate candidate, CancellationToken ct)
    {
        var arguments = new List<string>
        {
            "-hide_banner", "-loglevel", "error", "-nostdin",
            "-f", "lavfi", "-i", "testsrc2=size=640x360:rate=30:duration=1"
        };

        if (candidate.Name == "h264_vaapi")
        {
            var device = VaapiDevice();
            if (device is null) return false;

            arguments.AddRange(["-vaapi_device", device, "-vf", "format=nv12,hwupload"]);
        }

        arguments.AddRange(["-frames:v", "20", "-c:v", candidate.Name, "-f", "null", "-"]);

        try
        {
            var result = await ProcessRunner.RunAsync(ffmpegPath, arguments, timeout: TimeSpan.FromMinutes(2), ct: ct);

            if (!result.Success) return false;

            var error = result.Error;

            return !error.Contains("Cannot load", StringComparison.OrdinalIgnoreCase)
                && !error.Contains("No capable devices", StringComparison.OrdinalIgnoreCase)
                && !error.Contains("not supported", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            return false;
        }
    }

    private static string? VaapiDevice()
    {
        foreach (var device in new[] { "/dev/dri/renderD128", "/dev/dri/renderD129" })
        {
            if (File.Exists(device)) return device;
        }

        return null;
    }
}
