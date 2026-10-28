namespace Edutor.App.Shared.Services;

public sealed class AudioPlayerService
{
    public string? Url { get; private set; }
    public string? Title { get; private set; }
    public bool IsActive => !string.IsNullOrEmpty(Url);

    public event Action? Changed;
    public event Action? Ended;

    public void NotifyEnded() => Ended?.Invoke();

    public void Play(string url, string? title)
    {
        Url = url;
        Title = title;
        Changed?.Invoke();
    }

    public void Stop()
    {
        Url = null;
        Title = null;
        Changed?.Invoke();
    }
}
