using System.Globalization;
using Kela.Nodes.Shared.Encoding;

namespace Kela.Nodes.Worker.Encoding;

public sealed record EncodeRequest
{
    public string Input { get; init; } = string.Empty;

    public string OutputDirectory { get; init; } = string.Empty;

    public EncodingProfile Profile { get; init; } = new();

    public EncoderChoice Encoder { get; init; } = new("libx264", "cpu", false);

    /// <summary>NVENC üçün hansı GPU işlədilsin; -1 = ffmpeg özü seçir.</summary>
    public int GpuIndex { get; init; } = -1;

    /// <summary>Mənbə ölçüləri — upscale etməmək üçün.</summary>
    public int SourceHeight { get; init; }

    public double SourceFps { get; init; }

    public int VideoStreamIndex { get; init; }

    public int AudioStreamIndex { get; init; } = -1;

    public int SegmentSeconds { get; init; } = VideoLadder.SegmentSeconds;

    /// <summary>Faiz hesablanması üçün mənbə müddəti (saniyə).</summary>
    public double DurationSeconds { get; init; }

    /// <summary>0 = ffmpeg özü qərar verir.</summary>
    public int Threads { get; init; }

    /// <summary>Resurs profilindən gələn preset (məs. veryfast, p4). Boşdursa encoder default-u.</summary>
    public string Preset { get; init; } = string.Empty;

    /// <summary>Aşağı prioritetlə işlət — cihaz istifadəçiyə qalsın.</summary>
    public bool BelowNormalPriority { get; init; }
}

/// <summary>FFmpeg əmr sətirlərini qurur. Arqumentlər ayrı-ayrı verilir — shell yoxdur.</summary>
public static class EncodePlan
{
    private const int MaxFps = 60;

    public static IReadOnlyList<string> Hls(EncodeRequest request)
    {
        var profile = request.Profile;
        var height = EffectiveHeight(profile.Height, request.SourceHeight);
        var fps = EffectiveFps(request.SourceFps);

        var arguments = new List<string>
        {
            "-hide_banner", "-nostdin", "-loglevel", "error", "-y"
        };

        if (request.Encoder.Name == "h264_vaapi")
        {
            var device = VaapiDevice();
            if (device is not null) arguments.AddRange(["-vaapi_device", device]);
        }

        arguments.AddRange(["-i", request.Input]);

        // Yalnız bir video və bir audio axını götürülür: subtitle, attachment və
        // metadata axınları heç vaxt çıxışa düşmür.
        arguments.AddRange(["-map", $"0:{request.VideoStreamIndex}"]);

        if (request.AudioStreamIndex >= 0)
        {
            arguments.AddRange(["-map", $"0:{request.AudioStreamIndex}"]);
        }

        arguments.AddRange(["-vf", ScaleFilter(height, request.Encoder)]);

        if (fps != request.SourceFps && fps > 0) arguments.AddRange(["-r", fps.ToString(CultureInfo.InvariantCulture)]);

        if (request.Threads > 0) arguments.AddRange(["-threads", request.Threads.ToString(CultureInfo.InvariantCulture)]);

        AppendVideoEncoder(arguments, request.Encoder, profile, request.Preset, request.GpuIndex);

        if (request.AudioStreamIndex >= 0)
        {
            arguments.AddRange(["-c:a", "aac", "-b:a", $"{profile.AudioBitrateKbps}k", "-ac", "2"]);
        }
        else
        {
            arguments.Add("-an");
        }

        // Seqmentlər keyframe üzərində bölünsün deyə GOP uzunluğu seqmentə bərabər tutulur.
        var gop = fps > 0 ? (int)Math.Round(fps * request.SegmentSeconds) : request.SegmentSeconds * 25;

        arguments.AddRange(["-g", gop.ToString(CultureInfo.InvariantCulture)]);
        arguments.AddRange(["-keyint_min", gop.ToString(CultureInfo.InvariantCulture)]);
        arguments.AddRange(["-sc_threshold", "0"]);

        arguments.AddRange(
        [
            "-f", "hls",
            "-hls_time", request.SegmentSeconds.ToString(CultureInfo.InvariantCulture),
            "-hls_playlist_type", "vod",
            "-hls_flags", "independent_segments",
            "-hls_segment_type", "mpegts",
            "-hls_segment_filename", Path.Combine(request.OutputDirectory, "segment_%05d.ts"),
            Path.Combine(request.OutputDirectory, "index.m3u8")
        ]);

        return arguments;
    }

    public static IReadOnlyList<string> Thumbnail(string input, string output, int videoStreamIndex, double atSecond, int width = 640)
    {
        var arguments = new List<string> { "-hide_banner", "-nostdin", "-loglevel", "error", "-y" };

        if (atSecond > 0)
        {
            arguments.AddRange(["-ss", atSecond.ToString("0.###", CultureInfo.InvariantCulture)]);
        }

        arguments.AddRange(["-i", input, "-map", $"0:{videoStreamIndex}", "-frames:v", "1"]);
        arguments.AddRange(["-vf", $"scale={width}:-2"]);
        arguments.AddRange(["-q:v", "4", output]);

        return arguments;
    }

    /// <summary>Mənbədən böyük hündürlük istənilmir; H.264 üçün cüt ədədə yuvarlaqlaşdırılır.</summary>
    public static int EffectiveHeight(int targetHeight, int sourceHeight)
    {
        var height = sourceHeight > 0 ? Math.Min(targetHeight, sourceHeight) : targetHeight;
        return Math.Max(2, height - height % 2);
    }

    /// <summary>Kadr sürəti qaldırılmır — yalnız həddən artıq yüksək dəyərlər məhdudlaşdırılır.</summary>
    public static double EffectiveFps(double sourceFps)
        => sourceFps > MaxFps ? MaxFps : sourceFps;

    private static string ScaleFilter(int height, EncoderChoice encoder)
        => encoder.Name == "h264_vaapi"
            ? $"scale=-2:{height},format=nv12,hwupload"
            : $"scale=-2:{height}";

    private static void AppendVideoEncoder(
        List<string> arguments, EncoderChoice encoder, EncodingProfile profile, string preset, int gpuIndex)
    {
        arguments.AddRange(["-c:v", encoder.Name]);

        switch (encoder.Name)
        {
            case "libx264":
                arguments.AddRange(["-preset", Fallback(preset, "veryfast"), "-profile:v", "high"]);
                break;

            case "h264_nvenc":
                if (gpuIndex >= 0) arguments.AddRange(["-gpu", gpuIndex.ToString(CultureInfo.InvariantCulture)]);

                arguments.AddRange(["-preset", Fallback(preset, "p4"), "-rc", "vbr", "-profile:v", "high"]);
                break;

            case "h264_qsv":
                arguments.AddRange(["-preset", "veryfast", "-profile:v", "high"]);
                break;

            case "h264_videotoolbox":
                arguments.AddRange(["-profile:v", "high", "-realtime", "false"]);
                break;

            case "h264_amf":
                arguments.AddRange(["-quality", "balanced", "-usage", "transcoding"]);
                break;

            case "h264_mediacodec":
                arguments.AddRange(["-profile:v", "high"]);
                break;
        }

        arguments.AddRange(
        [
            "-b:v", $"{profile.VideoBitrateKbps}k",
            "-maxrate", $"{profile.EffectiveMaxRateKbps}k",
            "-bufsize", $"{profile.EffectiveBufSizeKbps}k"
        ]);

        // Hər seqmentin ilk kadrı açar kadr olsun deyə səhnə dəyişikliyi açar kadrı söndürülür.
        arguments.AddRange(["-pix_fmt", "yuv420p"]);
    }

    private static string Fallback(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static string? VaapiDevice()
    {
        foreach (var device in new[] { "/dev/dri/renderD128", "/dev/dri/renderD129" })
        {
            if (File.Exists(device)) return device;
        }

        return null;
    }
}
