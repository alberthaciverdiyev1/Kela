using System.Diagnostics;
using System.Globalization;

namespace Edutor.Nodes.Worker.Encoding;

/// <summary>
/// Real kodlaşdırma sürətini ölçür: sintetik 720p klip seçilmiş encoder ilə kodlaşdırılır
/// və saniyədə kadr hesablanır. Scheduler cihazları bu balla müqayisə edir.
/// </summary>
public sealed class BenchmarkRunner(string ffmpegPath, WorkerLog log)
{
    private const int TestSeconds = 10;
    private const int TestHeight = 720;
    private const int TestFps = 30;

    public async Task<Shared.Contracts.NodeBenchmarkResult> RunAsync(EncoderChoice encoder, CancellationToken ct = default)
    {
        var arguments = new List<string>
        {
            "-hide_banner", "-nostdin", "-loglevel", "error",
            "-f", "lavfi", "-i", $"testsrc2=size=1280x{TestHeight}:rate={TestFps}:duration={TestSeconds}"
        };

        if (encoder.Name == "h264_vaapi")
        {
            arguments.AddRange(["-vf", "format=nv12,hwupload"]);
        }

        arguments.AddRange(["-c:v", encoder.Name, "-b:v", "2500k", "-f", "null", "-"]);

        var stopwatch = Stopwatch.StartNew();
        var result = await ProcessRunner.RunAsync(ffmpegPath, arguments, timeout: TimeSpan.FromMinutes(10), ct: ct);
        stopwatch.Stop();

        var frames = TestSeconds * TestFps;

        if (!result.Success || stopwatch.Elapsed.TotalSeconds <= 0)
        {
            log.Warn("benchmark", "Sınaq kodlaşdırması alınmadı — default bal istifadə olunur");

            return new Shared.Contracts.NodeBenchmarkResult
            {
                Encoder = encoder.Name,
                Fps = 0,
                Score = 100,
                TestHeight = TestHeight,
                TestSeconds = TestSeconds
            };
        }

        var fps = frames / stopwatch.Elapsed.TotalSeconds;

        log.Info("benchmark", $"{encoder.Name}: {fps.ToString("0.0", CultureInfo.InvariantCulture)} fps ({TestHeight}p)");

        return new Shared.Contracts.NodeBenchmarkResult
        {
            Encoder = encoder.Name,
            Fps = Math.Round(fps, 2),
            Score = Score(fps),
            TestHeight = TestHeight,
            TestSeconds = TestSeconds
        };
    }

    /// <summary>Sürəti 1..10000 aralığında bir bala çevirir — scheduler yalnız nisbəti istifadə edir.</summary>
    private static int Score(double fps) => Math.Clamp((int)Math.Round(fps * 10), 1, 10_000);
}
