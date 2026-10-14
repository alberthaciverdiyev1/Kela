using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Lessons;

public partial class LessonContentDetail
{
    [Parameter] public long LessonId { get; set; }
    [Parameter] public long ContentId { get; set; }

    private bool _loading = true;
    private ContentItemDto? _content;
    private List<ContentItemDto> _siblings = new();
    private bool _done;
    private ElementReference _videoRef;
    private string? _url;
    private string? _attached;

    private bool IsVideo => _content?.Type == "video";
    private bool IsVoice => _content?.Type == "voice";

    private string RelatedTitle => "Dərs materialları";

    private static string IconFor(string? type) => type switch
    {
        "video" => "play",
        "voice" => "chart",
        _ => "file-text"
    };

    protected override void OnInitialized() => Player.Ended += OnAudioEnded;

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        _done = false;
        _attached = null;
        try
        {
            var all = await Api.ContentsAsync(LessonId);
            _content = all.FirstOrDefault(c => c.Id == ContentId);
            _siblings = _content is null
                ? new()
                : all.Where(c => c.Id != ContentId).ToList();
            _url = Absolute(_content?.PlaybackUrl ?? _content?.Url);
        }
        catch { }
        _loading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsVideo && !string.IsNullOrEmpty(_url) && _attached != _url)
        {
            _attached = _url;
            await JS.InvokeVoidAsync("lmsVideoPlayer.attach", _videoRef, _url);
        }
    }

    private void PlayAudio()
    {
        if (_content is null || string.IsNullOrEmpty(_url)) return;
        Player.Play(_url, _content.Title);
    }

    private void OnAudioEnded() => _ = InvokeAsync(async () =>
    {
        if (IsVoice) await MarkAsync();
    });

    private void OpenDocument()
    {
        if (_url is null || _content is null) return;
        Nav.NavigateTo($"/document?url={Uri.EscapeDataString(_url)}&title={Uri.EscapeDataString(_content.Title)}");
    }

    private async Task MarkAsync()
    {
        if (_content is null || _done) return;
        try
        {
            await Api.MarkCompletedAsync(_content.Id);
            _done = true;
        }
        catch { }
    }

    private static string Absolute(string? url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        if (url.StartsWith("http")) return url;
        return $"{AppConfig.ApiBaseUrl}/{url.TrimStart('/')}";
    }

    public void Dispose() => Player.Ended -= OnAudioEnded;
}
