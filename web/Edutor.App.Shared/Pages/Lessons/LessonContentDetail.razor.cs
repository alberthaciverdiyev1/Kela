using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Dtos.StudentNote;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Lessons;

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
    private long? _attachedKey;

    private List<StudentNoteDto> _notes = new();
    private List<GroupDto> _noteGroups = new();
    private bool _noteFormOpen;
    private string _noteBody = "";
    private string _noteVisibility = "private";
    private long? _noteGroupId;
    private int? _noteAt;
    private string? _noteError;
    private bool _noteSaving;
    private StudentNoteDto? _noteToDelete;
    private bool _noteDeleting;

    private record AudioState(double current, double duration, bool paused, double rate);

    private bool IsVideo => _content?.Type == "video";
    private bool IsVoice => _content?.Type == "voice";

    private string RelatedTitle => _content?.Type switch
    {
        "video" => L["Related_Videos"],
        "voice" => L["Related_Voices"],
        _ => L["Related_Documents"]
    };

    private static string IconFor(string? type) => type switch
    {
        "video" => "play",
        "voice" => "chart",
        _ => "file-text"
    };

    /// <summary>Kadr önizləməsi — videonun ilk saniyəsindən statik kadr.</summary>
    private static string Preview(string? url) => string.IsNullOrEmpty(url) ? "" : url + "#t=0.1";

    protected override void OnInitialized() => Player.Ended += OnAudioEnded;

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        _done = false;
        _attachedKey = null;
        _noteFormOpen = false;
        _notes = new();
        try
        {
            var all = await Api.ContentsAsync(LessonId);
            _content = all.FirstOrDefault(c => c.Id == ContentId);
            _siblings = _content is null
                ? new()
                : all.Where(c => c.Id != ContentId && c.Type == _content.Type).ToList();
            _url = Absolute(_content?.PlaybackUrl ?? _content?.Url);

            if (_content is not null && (IsVideo || IsVoice))
            {
                try { _notes = await Api.NotesAsync(contentId: ContentId); }
                catch { }
            }
        }
        catch { }
        _loading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_loading && IsVideo && !string.IsNullOrEmpty(_url) && _attachedKey != ContentId)
        {
            _attachedKey = ContentId;
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

    private async Task StartNoteAsync()
    {
        _noteAt = await CurrentSecondsAsync();
        _noteBody = "";
        _noteVisibility = "private";
        _noteGroupId = null;
        _noteError = null;
        _noteFormOpen = true;

        if (_noteGroups.Count == 0)
        {
            try { _noteGroups = await Api.StudentGroupsAsync(); }
            catch { }
            _noteGroupId = _noteGroups.FirstOrDefault()?.Id;
        }
    }

    private void SetNoteVisibility(string visibility)
    {
        _noteVisibility = visibility;
        if (visibility == "group" && _noteGroupId is null) _noteGroupId = _noteGroups.FirstOrDefault()?.Id;
    }

    private async Task<int?> CurrentSecondsAsync()
    {
        try
        {
            if (IsVideo)
            {
                var t = await JS.InvokeAsync<double>("lmsVideoPlayer.time");
                return t > 0.5 ? (int)t : null;
            }

            if (IsVoice && Player.IsActive)
            {
                var st = await JS.InvokeAsync<AudioState>("lmsAudio.state");
                return st.current > 0.5 ? (int)st.current : null;
            }
        }
        catch { }
        return null;
    }

    private async Task SaveNoteAsync()
    {
        if (_noteSaving) return;
        if (string.IsNullOrWhiteSpace(_noteBody))
        {
            _noteError = L["Notes_BodyRequired"];
            return;
        }

        _noteSaving = true;
        _noteError = null;
        try
        {
            var dto = new StudentNoteUpsertDto(
                ContentId,
                _noteAt,
                null,
                _noteBody.Trim(),
                _noteVisibility,
                _noteVisibility == "group" ? _noteGroupId : null);

            var created = await Api.CreateNoteAsync(dto);
            _notes.Insert(0, created);
            _noteFormOpen = false;
        }
        catch (ApiException ex)
        {
            _noteError = ex.Message;
        }
        catch (Exception ex)
        {
            _noteError = ex.Message;
        }
        finally
        {
            _noteSaving = false;
        }
    }

    private async Task JumpAsync(StudentNoteDto note)
    {
        if (note.PositionSeconds is not int seconds) return;
        try
        {
            if (IsVideo)
            {
                await JS.InvokeVoidAsync("lmsVideoPlayer.seek", seconds);
            }
            else if (IsVoice && _content is not null && !string.IsNullOrEmpty(_url))
            {
                if (Player.Url != _url) Player.Play(_url, _content.Title);
                Player.RequestSeek(seconds);
            }
        }
        catch { }
    }

    private async Task ConfirmDeleteNoteAsync()
    {
        if (_noteToDelete is null || _noteDeleting) return;

        var note = _noteToDelete;
        _noteDeleting = true;
        _noteError = null;
        try
        {
            await Api.DeleteNoteAsync(note.Id);
            _notes.RemoveAll(n => n.Id == note.Id);
            _noteToDelete = null;
        }
        catch (ApiException ex)
        {
            _noteError = ex.Message;
            _noteToDelete = null;
        }
        catch
        {
            _noteToDelete = null;
        }
        finally
        {
            _noteDeleting = false;
        }
    }

    private static string FormatTime(int seconds)
        => TimeSpan.FromSeconds(seconds).ToString(seconds >= 3600 ? @"h\:mm\:ss" : @"m\:ss");

    private static string Absolute(string? url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        if (url.StartsWith("http")) return url;
        return $"{AppConfig.ApiBaseUrl}/{url.TrimStart('/')}";
    }

    public void Dispose() => Player.Ended -= OnAudioEnded;
}
