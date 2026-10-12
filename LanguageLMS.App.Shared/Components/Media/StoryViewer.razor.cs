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

public partial class StoryViewer
{
    [Parameter, EditorRequired] public IReadOnlyList<BannerDto> Banners { get; set; } = Array.Empty<BannerDto>();
    [Parameter] public int StartIndex { get; set; }
    [Parameter] public int DurationSeconds { get; set; } = 5;
    [Parameter] public EventCallback OnClose { get; set; }

    private int _index;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    protected override void OnInitialized()
    {
        _index = Math.Clamp(StartIndex, 0, Math.Max(0, Banners.Count - 1));
        RestartTimer();
    }

    private void RestartTimer()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _ = AutoAdvanceAsync(_cts.Token);
    }

    private async Task AutoAdvanceAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(DurationSeconds), token);
            if (token.IsCancellationRequested || _disposed) return;
            await InvokeAsync(Next);
        }
        catch (TaskCanceledException) { }
    }

    private async Task Next()
    {
        if (_index >= Banners.Count - 1)
        {
            await CloseAsync();
            return;
        }

        _index++;
        RestartTimer();
        StateHasChanged();
    }

    private void Prev()
    {
        if (_index > 0) _index--;
        RestartTimer();
        StateHasChanged();
    }

    private async Task CloseAsync() => await OnClose.InvokeAsync();

    public void Dispose()
    {
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
