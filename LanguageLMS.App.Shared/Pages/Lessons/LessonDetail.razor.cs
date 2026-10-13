using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Lessons;

public partial class LessonDetail
{
    [Parameter] public long LessonId { get; set; }
    private bool _loading = true;
    private string? Title;
    private List<ContentItemDto> _contents = new();
    private ContentItemDto? _video;
    private List<ContentItemDto> _others = new();
    private List<QuizDto> _quizzes = new();
    private readonly HashSet<long> _completed = new();
    private ElementReference _videoRef;
    private string? _videoUrl;
    private string? _attached;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Title = (await Api.LessonAsync(LessonId)).Title;
            _contents = await Api.ContentsAsync(LessonId);
            _quizzes = await Api.LessonQuizzesAsync(LessonId);
            _video = _contents.FirstOrDefault(c => c.Type == "video");
            _others = _contents.Where(c => !ReferenceEquals(c, _video)).ToList();
            if (_video is not null)
            {
                _videoUrl = Absolute(_video.PlaybackUrl ?? _video.Url);
            }
        }
        catch { }
        _loading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_video is not null && !string.IsNullOrEmpty(_videoUrl) && _attached != _videoUrl)
        {
            _attached = _videoUrl;
            await JS.InvokeVoidAsync("lmsVideoPlayer.attach", _videoRef, _videoUrl);
        }
    }

    private bool IsDone(long id) => _completed.Contains(id);

    /// <summary>Video sona çatanda materialı avtomatik "tamamlandı" işarələ.</summary>
    private async Task OnVideoEndedAsync()
    {
        if (_video is not null && !IsDone(_video.Id))
        {
            await MarkAsync(_video.Id);
        }
    }

    private async Task MarkAsync(long contentId)
    {
        try
        {
            await Api.MarkCompletedAsync(contentId);
            _completed.Add(contentId);
        }
        catch { }
    }

    private void Open(ContentItemDto c)
    {
        var url = Absolute(c.PlaybackUrl ?? c.Url);
        if (string.IsNullOrEmpty(url)) return;

        // Səs → alt bölmədəki pleyer; digərləri (pdf/şəkil) → tam səhifə sənəd görüntüləyicisi
        if (c.Type == "voice")
        {
            Player.Play(url, c.Title);
            return;
        }

        Nav.NavigateTo($"/document?url={Uri.EscapeDataString(url)}&title={Uri.EscapeDataString(c.Title)}");
    }

    private static string Absolute(string? url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        if (url.StartsWith("http")) return url;
        return $"{AppConfig.ApiBaseUrl}/{url.TrimStart('/')}";
    }
}
