using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Components.Media;

public partial class AudioPlayerBar
{
    private static readonly double[] Rates = [1, 1.25, 1.5, 2];

    private double _current;
    private double _duration;
    private double _rate = 1;
    private bool _paused = true;
    private bool _wasPlaying;
    private string? _loadedUrl;
    private CancellationTokenSource? _cts;

    private record AudioState(double current, double duration, bool paused, double rate);

    protected override void OnInitialized()
    {
        Player.Changed += OnChanged;
        _ = InvokeAsync(SyncAsync);
        StartPolling();
    }

    private void OnChanged() => _ = InvokeAsync(async () => { await SyncAsync(); StateHasChanged(); });

    private void StartPolling()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await InvokeAsync(async () =>
                {
                    if (!Player.IsActive) return;
                    try
                    {
                        var st = await JS.InvokeAsync<AudioState>("lmsAudio.state");
                        _current = st.current;
                        _duration = st.duration;
                        _paused = st.paused;
                        _rate = st.rate;

                        if (!st.paused) _wasPlaying = true;
                        else if (_wasPlaying && st.duration > 0 && st.current >= st.duration - 0.5)
                        {
                            _wasPlaying = false;
                            Player.NotifyEnded();
                        }

                        StateHasChanged();
                    }
                    catch { /* JS hazır deyil */ }
                });
                try { await Task.Delay(300, token); } catch { break; }
            }
        });
    }

    private async Task SyncAsync()
    {
        if (!Player.IsActive)
        {
            _loadedUrl = null;
            _current = 0;
            _duration = 0;
            _paused = true;
            return;
        }

        if (_loadedUrl != Player.Url)
        {
            _loadedUrl = Player.Url;
            await JS.InvokeVoidAsync("lmsAudio.load", Player.Url);
            await JS.InvokeVoidAsync("lmsAudio.setRate", _rate);
            await JS.InvokeVoidAsync("lmsAudio.play");
            _paused = false;
        }
    }

    private async Task ToggleAsync()
    {
        if (_paused)
        {
            await JS.InvokeVoidAsync("lmsAudio.play");
            _paused = false;
        }
        else
        {
            await JS.InvokeVoidAsync("lmsAudio.pause");
            _paused = true;
        }
    }

    private async Task SeekAsync(ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out var t))
        {
            await JS.InvokeVoidAsync("lmsAudio.seek", t);
            _current = t;
        }
    }

    private async Task SetRateAsync(double rate)
    {
        _rate = rate;
        await JS.InvokeVoidAsync("lmsAudio.setRate", rate);
    }

    private async Task StopAsync()
    {
        await JS.InvokeVoidAsync("lmsAudio.stop");
        _loadedUrl = null;
        Player.Stop();
    }

    private static string Fmt(double seconds)
    {
        if (seconds <= 0 || double.IsNaN(seconds)) return "0:00";
        var t = TimeSpan.FromSeconds(seconds);
        return t.TotalHours >= 1
            ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}"
            : $"{t.Minutes}:{t.Seconds:00}";
    }

    private static string FmtRate(double r) => r == Math.Floor(r) ? $"{(int)r}x" : $"{r.ToString(System.Globalization.CultureInfo.InvariantCulture)}x";

    public void Dispose()
    {
        Player.Changed -= OnChanged;
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
