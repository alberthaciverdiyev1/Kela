using Kela.Nodes.Shared.Encoding;
using Kela.Nodes.Worker;
using Kela.Nodes.Worker.Encoding;

namespace Kela.Tests.Nodes;

/// <summary>FFmpeg əmr qurucusunun qaydaları — upscale etməmək, axın seçimi.</summary>
public class EncodePlanTests
{
    private static EncodeRequest Request(int targetHeight, int sourceHeight, bool withAudio = true) => new()
    {
        Input = "/tmp/in.mp4",
        OutputDirectory = "/tmp/out",
        Profile = new EncodingProfile { Name = "720p", Height = targetHeight, VideoBitrateKbps = 2500 },
        Encoder = new EncoderChoice("libx264", "cpu", false),
        SourceHeight = sourceHeight,
        SourceFps = 25,
        VideoStreamIndex = 1,
        AudioStreamIndex = withAudio ? 2 : -1
    };

    [Theory]
    [InlineData(1080, 480, 480)]
    [InlineData(720, 720, 720)]
    [InlineData(360, 2160, 360)]
    [InlineData(1080, 0, 1080)]
    public void Requested_height_is_capped_by_the_source(int target, int source, int expected)
        => Assert.Equal(expected, EncodePlan.EffectiveHeight(target, source));

    [Fact]
    public void Effective_height_is_always_even()
    {
        Assert.Equal(480, EncodePlan.EffectiveHeight(481, 0));
        Assert.Equal(360, EncodePlan.EffectiveHeight(361, 0));
    }

    [Fact]
    public void Frame_rate_is_never_raised_and_is_capped()
    {
        Assert.Equal(25, EncodePlan.EffectiveFps(25));
        Assert.Equal(60, EncodePlan.EffectiveFps(120));
        Assert.Equal(0, EncodePlan.EffectiveFps(0));
    }

    [Fact]
    public void Only_the_probed_video_and_audio_streams_are_mapped()
    {
        var arguments = EncodePlan.Hls(Request(720, 1080)).ToList();

        // Albom qapağı ola biləcəyi üçün "0:v:0" yox, probedən gələn indeks istifadə olunur.
        Assert.Equal("0:1", arguments[arguments.IndexOf("-map") + 1]);
        Assert.DoesNotContain("0:s", arguments);
        Assert.DoesNotContain("0:t", arguments);
        Assert.Equal("0:2", arguments[arguments.IndexOf("-map", arguments.IndexOf("-map") + 1) + 1]);
    }

    [Fact]
    public void A_silent_source_produces_no_audio_stream()
    {
        var arguments = EncodePlan.Hls(Request(720, 1080, withAudio: false)).ToList();

        Assert.Contains("-an", arguments);
        Assert.DoesNotContain("-c:a", arguments);
        Assert.Single(arguments.Where(a => a == "-map"));
    }

    [Fact]
    public void The_ladder_profile_settings_reach_ffmpeg()
    {
        var arguments = EncodePlan.Hls(Request(720, 1080)).ToList();

        Assert.Contains("2500k", arguments);
        Assert.Contains("3750k", arguments);   // maxrate = 1.5×
        Assert.Contains("5000k", arguments);   // bufsize = 2×
        Assert.Contains("-hls_playlist_type", arguments);
        Assert.Contains("vod", arguments);
    }

    [Fact]
    public void Keyframes_are_aligned_to_the_segment_length()
    {
        var request = Request(720, 1080) with { SegmentSeconds = 8, SourceFps = 25 };
        var arguments = EncodePlan.Hls(request).ToList();

        // 25 fps × 8 saniyə = 200 kadr; seqmentlər açar kadrda bölünsün.
        Assert.Equal("200", arguments[arguments.IndexOf("-g") + 1]);
        Assert.Equal("8", arguments[arguments.IndexOf("-hls_time") + 1]);
    }

    [Fact]
    public void The_thumbnail_is_scaled_and_produced_from_the_probed_stream()
    {
        var arguments = EncodePlan.Thumbnail("/tmp/in.mp4", "/tmp/thumb.jpg", 1, 30).ToList();

        Assert.Equal("0:1", arguments[arguments.IndexOf("-map") + 1]);
        Assert.Equal("scale=640:-2", arguments[arguments.IndexOf("-vf") + 1]);
    }
}

/// <summary>
/// Real ffmpeg ilə kodlaşdırma. ffmpeg olmayan mühitdə test erkən çıxır —
/// bu halda nəticə "keçdi" kimi görünür, ona görə CI-də ffmpeg quraşdırılmalıdır.
/// </summary>
public class FfmpegIntegrationTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"kela-ffmpeg-{Guid.NewGuid():N}");
    private readonly WorkerLog _log = new();

    private readonly string? _ffmpeg = Executable("ffmpeg");
    private readonly string? _ffprobe = Executable("ffprobe");

    public FfmpegIntegrationTests() => Directory.CreateDirectory(_directory);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // Keş qovluğu qalsın — test nəticəsinə təsiri yoxdur.
        }

        GC.SuppressFinalize(this);
    }

    private static string? Executable(string name)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

        return path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => Path.Combine(directory.Trim(), name))
            .FirstOrDefault(File.Exists);
    }

    /// <summary>Sintetik mənbə: video + audio, ya da yalnız video.</summary>
    private async Task<string> CreateSourceAsync(bool withAudio, int seconds = 4)
    {
        var output = Path.Combine(_directory, withAudio ? "with-audio.mp4" : "silent.mp4");

        var arguments = new List<string>
        {
            "-hide_banner", "-nostdin", "-loglevel", "error", "-y",
            "-f", "lavfi", "-i", $"testsrc2=size=640x480:rate=25:duration={seconds}"
        };

        if (withAudio)
        {
            arguments.AddRange(["-f", "lavfi", "-i", $"sine=frequency=440:duration={seconds}", "-shortest", "-c:a", "aac"]);
        }

        arguments.AddRange(["-c:v", "libx264", "-preset", "ultrafast", output]);

        var result = await ProcessRunner.RunAsync(_ffmpeg!, arguments, timeout: TimeSpan.FromMinutes(2));

        Assert.True(result.Success, result.Error);

        return output;
    }

    [Fact]
    public async Task Probing_reads_resolution_frame_rate_and_audio()
    {
        if (_ffprobe is null) return;

        var source = await CreateSourceAsync(withAudio: true);
        var media = await new MediaProbe(_ffprobe).ProbeAsync(source);

        Assert.NotNull(media);
        Assert.Equal(640, media!.Width);
        Assert.Equal(480, media.Height);
        Assert.InRange(media.Fps, 24.5, 25.5);
        Assert.True(media.HasAudio);
        Assert.InRange(media.DurationSeconds, 3, 5);
    }

    [Fact]
    public async Task A_silent_source_is_reported_as_having_no_audio()
    {
        if (_ffprobe is null) return;

        var source = await CreateSourceAsync(withAudio: false);
        var media = await new MediaProbe(_ffprobe).ProbeAsync(source);

        Assert.NotNull(media);
        Assert.False(media!.HasAudio);
        Assert.Equal(-1, media.AudioStreamIndex);
    }

    [Fact]
    public async Task Encoding_produces_a_playable_vod_playlist_with_aligned_segments()
    {
        if (_ffmpeg is null || _ffprobe is null) return;

        var source = await CreateSourceAsync(withAudio: true);
        var media = await new MediaProbe(_ffprobe).ProbeAsync(source);
        var output = Path.Combine(_directory, "360p");

        var outcome = await new HlsEncoder(_ffmpeg, _log).EncodeAsync(new EncodeRequest
        {
            Input = source,
            OutputDirectory = output,
            Profile = new EncodingProfile { Name = "360p", Height = 360, VideoBitrateKbps = 700 },
            Encoder = new EncoderChoice("libx264", "cpu", false),
            SourceHeight = media!.Height,
            SourceFps = media.Fps,
            DurationSeconds = media.DurationSeconds,
            VideoStreamIndex = media.VideoStreamIndex,
            AudioStreamIndex = media.AudioStreamIndex,
            SegmentSeconds = 2
        });

        Assert.True(outcome.Success, outcome.Error);

        var playlist = await File.ReadAllTextAsync(Path.Combine(output, "index.m3u8"));

        Assert.StartsWith("#EXTM3U", playlist);
        Assert.Contains("#EXT-X-PLAYLIST-TYPE:VOD", playlist);
        Assert.Contains("#EXT-X-ENDLIST", playlist);

        var segments = playlist
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.EndsWith(".ts", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(segments);
        Assert.All(segments, segment => Assert.True(File.Exists(Path.Combine(output, segment)), $"çatışmır: {segment}"));

        // Aspect ratio qorunub: 640×480 mənbədən 360p → 480×360.
        var encoded = await new MediaProbe(_ffprobe).ProbeAsync(Path.Combine(output, segments[0]));
        Assert.NotNull(encoded);
        Assert.Equal(360, encoded!.Height);
        Assert.Equal(480, encoded.Width);
    }

    [Fact]
    public async Task A_source_below_the_target_resolution_is_not_upscaled()
    {
        if (_ffmpeg is null || _ffprobe is null) return;

        var source = await CreateSourceAsync(withAudio: false);
        var output = Path.Combine(_directory, "1080p");

        var outcome = await new HlsEncoder(_ffmpeg, _log).EncodeAsync(new EncodeRequest
        {
            Input = source,
            OutputDirectory = output,
            Profile = new EncodingProfile { Name = "1080p", Height = 1080, VideoBitrateKbps = 4500 },
            Encoder = new EncoderChoice("libx264", "cpu", false),
            SourceHeight = 480,
            SourceFps = 25,
            DurationSeconds = 4,
            VideoStreamIndex = 0,
            AudioStreamIndex = -1,
            SegmentSeconds = 2
        });

        Assert.True(outcome.Success, outcome.Error);

        var first = Directory.EnumerateFiles(output, "*.ts").OrderBy(f => f).First();
        var media = await new MediaProbe(_ffprobe).ProbeAsync(first);

        Assert.NotNull(media);
        Assert.Equal(480, media!.Height);
    }

    [Fact]
    public async Task A_thumbnail_is_written_for_the_video()
    {
        if (_ffmpeg is null || _ffprobe is null) return;

        var source = await CreateSourceAsync(withAudio: true);
        var thumbnail = Path.Combine(_directory, "thumb.jpg");

        var created = await new HlsEncoder(_ffmpeg, _log)
            .CreateThumbnailAsync(source, thumbnail, 0, 4);

        Assert.True(created);
        Assert.True(File.Exists(thumbnail));
        Assert.True(new FileInfo(thumbnail).Length > 0);
    }
}
