using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Services;

namespace Edutor.App.Shared.Components.Media;

public partial class AudioPlayerBar
{
    private static readonly double[] Rates = [1, 1.25, 1.5, 2];

    private double _current;
    private double _duration;
    private double _rate = 1;
    private bool _paused = true;
    private bool _wasPlaying;
    private bool _queueOpen;
    private bool _dragging;
    private string? _loadedKey;
    private ElementReference _trackRef;
    private ElementReference _barRef;
    private CancellationTokenSource? _cts;

    private record AudioState(double current, double duration, bool paused, double rate);

    private string LoadKey => $"{Player.Index}|{Player.Url}";

    private string PctValue => _duration > 0
        ? Math.Clamp(_current / _duration * 100, 0, 100).ToString("0.###", CultureInfo.InvariantCulture)
        : "0";

    protected override void OnInitialized()
    {
        Player.Changed += OnChanged;
        Player.SeekRequested += OnSeekRequested;
        _ = InvokeAsync(SyncAsync);
        StartPolling();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (Player.IsActive) await JS.InvokeVoidAsync("lmsAudio.reserveFab", _barRef);
            else await JS.InvokeVoidAsync("lmsAudio.releaseFab");
        }
        catch { /* JS hazır deyil */ }
    }

    private void OnChanged() => _ = InvokeAsync(async () => { await SyncAsync(); StateHasChanged(); });

    private void OnSeekRequested(double seconds) => _ = InvokeAsync(async () =>
    {
        if (!Player.IsActive) return;
        _loadedKey = LoadKey;
        _paused = false;
        try { await JS.InvokeVoidAsync("lmsAudio.loadAt", Player.Url, seconds); }
        catch { /* JS hazır deyil */ }
        StateHasChanged();
    });

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
                        // Sürükləmə zamanı serverin köhnə vaxtı thumb-u geri dartmamalıdır.
                        if (!_dragging)
                        {
                            _current = st.current;
                            _duration = st.duration;
                        }
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
            _loadedKey = null;
            _queueOpen = false;
            _current = 0;
            _duration = 0;
            _paused = true;
            return;
        }

        // Eyni URL-li iki səs ola bilər — yükləmə açarı indekslə birlikdə tutulur.
        if (_loadedKey != LoadKey)
        {
            _loadedKey = LoadKey;
            _current = 0;
            _duration = 0;
            _wasPlaying = false;
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

    private async Task OnPointerDownAsync(PointerEventArgs e)
    {
        _dragging = true;
        await SeekToPointerAsync(e);
    }

    private async Task OnPointerMoveAsync(PointerEventArgs e)
    {
        if (_dragging) await SeekToPointerAsync(e);
    }

    private void EndDrag() => _dragging = false;

    private async Task SeekToPointerAsync(PointerEventArgs e)
    {
        try
        {
            var ratio = await JS.InvokeAsync<double>("lmsAudio.seekFromPointer", _trackRef, e.ClientX);
            if (_duration > 0) _current = ratio * _duration;
            StateHasChanged();
        }
        catch { /* JS hazır deyil */ }
    }

    private async Task PlayAtAsync(int index)
    {
        Player.PlayAt(index);
        StateHasChanged();
    }

    private Task NextAsync() { Player.Next(); return Task.CompletedTask; }

    private Task PreviousAsync() { Player.Previous(); return Task.CompletedTask; }

    private async Task SetRateAsync(double rate)
    {
        _rate = rate;
        await JS.InvokeVoidAsync("lmsAudio.setRate", rate);
    }

    private async Task StopAsync()
    {
        await JS.InvokeVoidAsync("lmsAudio.stop");
        _loadedKey = null;
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

    private static string FmtRate(double r) => r == Math.Floor(r) ? $"{(int)r}x" : $"{r.ToString(CultureInfo.InvariantCulture)}x";

    public void Dispose()
    {
        Player.Changed -= OnChanged;
        Player.SeekRequested -= OnSeekRequested;
        _cts?.Cancel();
        _cts?.Dispose();
        _ = ReleaseFabAsync();
    }

    private async Task ReleaseFabAsync()
    {
        try { await JS.InvokeVoidAsync("lmsAudio.releaseFab"); } catch { }
    }
}
