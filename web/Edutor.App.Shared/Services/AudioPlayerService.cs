namespace Edutor.App.Shared.Services;

public sealed record AudioTrack(string Url, string Title, long? ContentId);

public sealed class AudioPlayerService
{
    private static readonly IReadOnlyList<AudioTrack> Empty = Array.Empty<AudioTrack>();

    public IReadOnlyList<AudioTrack> Tracks { get; private set; } = Empty;
    public int Index { get; private set; } = -1;
    public int TrackCount => Tracks.Count;

    public AudioTrack? Current => Index >= 0 && Index < Tracks.Count ? Tracks[Index] : null;

    public string? Url => Current?.Url;
    public string? Title => Current?.Title;
    public bool IsActive => !string.IsNullOrEmpty(Url);

    public event Action? Changed;
    public event Action<AudioTrack?>? Ended;
    public event Action<double>? SeekRequested;

    public void NotifyEnded()
    {
        var finished = Current;
        Ended?.Invoke(finished);

        if (Index + 1 < Tracks.Count)
        {
            Index++;
            Changed?.Invoke();
        }
    }

    public void RequestSeek(double seconds) => SeekRequested?.Invoke(seconds);

    public void Play(string url, string? title) => Play([new AudioTrack(url, title ?? "", null)], 0);

    public void Play(IReadOnlyList<AudioTrack> tracks, int index)
    {
        if (tracks.Count == 0) return;
        Tracks = tracks;
        Index = Math.Clamp(index, 0, tracks.Count - 1);
        Changed?.Invoke();
    }

    public void PlayAt(int index)
    {
        if (index < 0 || index >= Tracks.Count || index == Index) return;
        Index = index;
        Changed?.Invoke();
    }

    public void Next() => PlayAt(Index + 1);

    public void Previous() => PlayAt(Index - 1);

    public void Stop()
    {
        Tracks = Empty;
        Index = -1;
        Changed?.Invoke();
    }
}
