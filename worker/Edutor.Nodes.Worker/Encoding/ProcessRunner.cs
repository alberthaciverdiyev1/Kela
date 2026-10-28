using System.Diagnostics;
using System.Text;

namespace Edutor.Nodes.Worker.Encoding;

public sealed record ProcessResult(int ExitCode, string Output, string Error, bool TimedOut)
{
    public bool Success => ExitCode == 0 && !TimedOut;
}

/// <summary>
/// Xarici prosesləri <b>arqument siyahısı</b> ilə işə salır — heç vaxt shell stringi
/// qurulmur, ona görə fayl adlarındakı xüsusi simvollar əmr kimi icra oluna bilməz.
/// </summary>
public static class ProcessRunner
{
    public static async Task<ProcessResult> RunAsync(
        string executable,
        IEnumerable<string> arguments,
        Action<string>? onOutput = null,
        TimeSpan? timeout = null,
        bool belowNormalPriority = false,
        CancellationToken ct = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        var closed = new TaskCompletionSource();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) return;

            output.AppendLine(e.Data);
            onOutput?.Invoke(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) return;

            error.AppendLine(e.Data);
            onOutput?.Invoke(e.Data);
        };

        process.Exited += (_, _) => closed.TrySetResult();
        process.EnableRaisingEvents = true;

        process.Start();

        if (belowNormalPriority)
        {
            // "Aşağı resurs" profilində kodlaşdırma cihazın qalan hissəsinə mane olmasın.
            try
            {
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch (Exception ex) when (ex is InvalidOperationException or PlatformNotSupportedException or System.ComponentModel.Win32Exception)
            {
                // Bəzi platformalarda prioritet dəyişmək mümkün deyil — davam edilir.
            }
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutSource = timeout is null ? null : new CancellationTokenSource(timeout.Value);
        using var linked = timeoutSource is null
            ? CancellationTokenSource.CreateLinkedTokenSource(ct)
            : CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutSource.Token);

        var timedOut = false;

        try
        {
            await closed.Task.WaitAsync(linked.Token);
        }
        catch (OperationCanceledException)
        {
            timedOut = timeoutSource?.IsCancellationRequested == true && !ct.IsCancellationRequested;

            TryKill(process);

            if (!timedOut) throw;
        }

        // Çıxış axınlarının boşalmasını gözlə.
        try
        {
            process.WaitForExit();
        }
        catch (InvalidOperationException)
        {
            // Proses artıq yoxdur.
        }

        var exitCode = timedOut ? -1 : SafeExitCode(process);

        return new ProcessResult(exitCode, output.ToString(), error.ToString(), timedOut);
    }

    private static int SafeExitCode(Process process)
    {
        try
        {
            return process.ExitCode;
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            // Proses artıq bitib.
        }
    }
}
