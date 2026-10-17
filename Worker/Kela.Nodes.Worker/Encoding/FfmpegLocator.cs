namespace Kela.Nodes.Worker.Encoding;

/// <summary>ffmpeg / ffprobe ikililərinin yerini tapır.</summary>
public sealed class FfmpegLocator(WorkerSettings settings, WorkerLog log)
{
    private string? _ffmpeg;
    private string? _ffprobe;

    public string? FfmpegPath => _ffmpeg;

    public string? FfprobePath => _ffprobe;

    public bool IsAvailable => _ffmpeg is not null && _ffprobe is not null;

    public async Task<bool> LocateAsync(CancellationToken ct = default)
    {
        _ffmpeg = await FindAsync(settings.FfmpegPath, "FFMPEG_PATH", "ffmpeg", ct);
        _ffprobe = await FindAsync(settings.FfprobePath, "FFPROBE_PATH", "ffprobe", ct);

        if (_ffmpeg is null || _ffprobe is null)
        {
            log.Error("ffmpeg", "ffmpeg və ya ffprobe tapılmadı — konversiya mümkün deyil");
            return false;
        }

        var version = await ProcessRunner.RunAsync(_ffmpeg, ["-hide_banner", "-version"], timeout: TimeSpan.FromSeconds(15), ct: ct);
        log.Info("ffmpeg", version.Output.Split('\n').FirstOrDefault()?.Trim() ?? _ffmpeg);

        return true;
    }

    private async Task<string?> FindAsync(string configured, string environmentVariable, string name, CancellationToken ct)
    {
        foreach (var candidate in Candidates(configured, environmentVariable, name))
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            if (await WorksAsync(candidate, ct)) return candidate;
        }

        return null;
    }

    private IEnumerable<string> Candidates(string configured, string environmentVariable, string name)
    {
        if (!string.IsNullOrWhiteSpace(configured)) yield return configured;

        // Platformanın verdiyi qovluq (Android: native lib dir) ən etibarlı yerdir.
        // Android ikililəri "lib<ad>.so" kimi paketlənir — ona görə hər iki ad yoxlanılır.
        if (!string.IsNullOrWhiteSpace(settings.PlatformBinaryDirectory))
        {
            yield return Path.Combine(settings.PlatformBinaryDirectory, $"lib{name}.so");
            yield return Path.Combine(settings.PlatformBinaryDirectory, $"{name}.so");
            yield return Path.Combine(settings.PlatformBinaryDirectory, name);
        }

        var fromEnvironment = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrWhiteSpace(fromEnvironment)) yield return fromEnvironment;

        // PATH üzrə axtarış — Android-də app xüsusi qovluqları da yoxlanılır.
        foreach (var directory in SearchDirectories())
        {
            yield return Path.Combine(directory, name);
            if (OperatingSystem.IsWindows()) yield return Path.Combine(directory, name + ".exe");
        }
    }

    private static IEnumerable<string> SearchDirectories()
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            yield return directory.Trim();
        }

        if (OperatingSystem.IsAndroid())
        {
            // Android-də ffmpeg sistemdə olmur; operator onu app-ın öz qovluğuna qoyur.
            foreach (var directory in new[]
                     {
                         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bin"),
                         "/data/local/tmp",
                         "/system/bin"
                     })
            {
                yield return directory;
            }
        }

        if (OperatingSystem.IsWindows())
        {
            foreach (var directory in new[]
                     {
                         @"C:\ffmpeg\bin",
                         @"C:\Program Files\ffmpeg\bin",
                         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ffmpeg", "bin")
                     })
            {
                yield return directory;
            }
        }
    }

    private static async Task<bool> WorksAsync(string candidate, CancellationToken ct)
    {
        try
        {
            if (!OperatingSystem.IsWindows() && Path.IsPathRooted(candidate) && !File.Exists(candidate)) return false;

            var result = await ProcessRunner.RunAsync(candidate, ["-hide_banner", "-version"],
                timeout: TimeSpan.FromSeconds(10), ct: ct);

            return result.Success;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
