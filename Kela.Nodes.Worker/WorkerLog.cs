using System.Collections.Concurrent;

namespace Kela.Nodes.Worker;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public sealed record LogEntry(DateTimeOffset At, LogLevel Level, string Source, string Message)
{
    public string Line => $"{At:HH:mm:ss}  {Level.ToString().ToUpperInvariant(),-7} {Source,-10} {Message}";
}

/// <summary>
/// Yaddaşda saxlanılan halqavari jurnal. UI onu oxuyur; uzun kodlaşdırmalarda
/// fayl I/O-su encoding performansına mane olmasın deyə disk yazısı yoxdur.
/// </summary>
public sealed class WorkerLog(int capacity = 2000)
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public event Action<LogEntry>? Written;

    public void Write(LogLevel level, string source, string message)
    {
        var entry = new LogEntry(DateTimeOffset.Now, level, source, message);

        _entries.Enqueue(entry);

        while (_entries.Count > capacity && _entries.TryDequeue(out _))
        {
        }

        Written?.Invoke(entry);
    }

    public void Debug(string source, string message) => Write(LogLevel.Debug, source, message);

    public void Info(string source, string message) => Write(LogLevel.Info, source, message);

    public void Warn(string source, string message) => Write(LogLevel.Warning, source, message);

    public void Error(string source, string message) => Write(LogLevel.Error, source, message);

    public IReadOnlyList<LogEntry> Snapshot() => [.. _entries];

    public void Clear()
    {
        while (_entries.TryDequeue(out _))
        {
        }
    }
}
