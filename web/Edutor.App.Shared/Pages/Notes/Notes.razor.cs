using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.StudentNote;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Notes;

public partial class Notes
{
    private static readonly (string Key, string Value)[] Tabs =
    [
        ("Notes_All", ""),
        ("Notes_Mine", "mine"),
        ("Notes_Shared", "shared")
    ];

    // Klientdə kart rəngləri — server yalnız açar sözü saxlayır.
    private static readonly (string Key, string Hex)[] Palette =
    [
        ("default", "#ffffff"),
        ("red", "#f8b4b4"),
        ("orange", "#fdd9a0"),
        ("yellow", "#fff3a3"),
        ("green", "#c9f2b2"),
        ("teal", "#a9efe4"),
        ("blue", "#bfe3fb"),
        ("purple", "#dcc9f7"),
        ("pink", "#f9c9e0"),
        ("gray", "#e2e5e9"),
        ("brown", "#e3cbb0")
    ];

    private bool _loading = true;
    private List<StudentNoteDto> _notes = new();
    private List<GroupDto> _groups = new();

    private string _scope = "";
    private string _search = "";
    private bool _composerOpen;
    private long? _editingId;
    private string _title = "";
    private string _body = "";
    private string _visibility = "private";
    private long? _groupId;
    private string _draftColor = "default";
    private bool _draftPinned;
    private long? _formContentId;
    private int? _formPosition;
    private bool _saving;
    private string? _error;
    private long? _confirmDeleteId;

    protected override void OnInitialized() => OnRefresh(LoadAsync);

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
        _loading = false;
    }

    private static string Bg(string? color)
        => Palette.FirstOrDefault(p => p.Key == color).Hex ?? Palette[0].Hex;

    private List<StudentNoteDto> FilteredNotes()
    {
        var term = _search.Trim();
        if (term.Length == 0) return _notes;

        return _notes
            .Where(n => (n.Title ?? "").Contains(term, StringComparison.OrdinalIgnoreCase)
                        || n.Body.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task LoadAsync()
    {
        try
        {
            _notes = await Api.NotesAsync(scope: string.IsNullOrEmpty(_scope) ? null : _scope);
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
        catch { }

        if (_groups.Count == 0)
        {
            try { _groups = await Api.StudentGroupsAsync(); }
            catch { }
        }
    }

    private async Task SetScopeAsync(string scope)
    {
        _scope = scope;
        _loading = true;
        await LoadAsync();
        _loading = false;
    }

    private void OpenComposer()
    {
        ResetDraft();
        _composerOpen = true;
    }

    private void StartEdit(StudentNoteDto note)
    {
        _editingId = note.Id;
        _title = note.Title ?? "";
        _body = note.Body;
        _visibility = note.Visibility;
        _groupId = note.GroupId;
        _draftColor = note.Color;
        _draftPinned = note.IsPinned;
        _formContentId = note.ContentId;
        _formPosition = note.PositionSeconds;
        _error = null;
        _composerOpen = true;
    }

    private void CancelComposer()
    {
        ResetDraft();
        _composerOpen = false;
    }

    private void ResetDraft()
    {
        _editingId = null;
        _title = "";
        _body = "";
        _visibility = "private";
        _groupId = _groups.FirstOrDefault()?.Id;
        _draftColor = "default";
        _draftPinned = false;
        _formContentId = null;
        _formPosition = null;
        _error = null;
    }

    private void SetVisibility(string visibility)
    {
        _visibility = visibility;
        if (visibility == "group" && _groupId is null) _groupId = _groups.FirstOrDefault()?.Id;
    }

    private void ToggleDraftPin() => _draftPinned = !_draftPinned;

    private async Task SaveAsync()
    {
        if (_saving) return;
        if (string.IsNullOrWhiteSpace(_body))
        {
            _error = L["Notes_BodyRequired"];
            return;
        }

        _saving = true;
        _error = null;
        var dto = new StudentNoteUpsertDto(
            _formContentId,
            _formPosition,
            string.IsNullOrWhiteSpace(_title) ? null : _title.Trim(),
            _body.Trim(),
            _visibility,
            _visibility == "group" ? _groupId : null,
            _draftColor,
            _draftPinned);

        try
        {
            if (_editingId is long id) await Api.UpdateNoteAsync(id, dto);
            else await Api.CreateNoteAsync(dto);

            _composerOpen = false;
            ResetDraft();
            _loading = true;
            await LoadAsync();
            _loading = false;
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task TogglePinAsync(StudentNoteDto note)
    {
        try
        {
            var updated = await Api.PinNoteAsync(note.Id, !note.IsPinned);
            Replace(updated);
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
    }

    private async Task SetColorAsync(StudentNoteDto note, string color)
    {
        try
        {
            var updated = await Api.SetNoteColorAsync(note.Id, color);
            Replace(updated);
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
    }

    private void Replace(StudentNoteDto updated)
    {
        var index = _notes.FindIndex(n => n.Id == updated.Id);
        if (index >= 0) _notes[index] = updated;
    }

    private void AskDelete(long id) => _confirmDeleteId = id;

    private async Task DeleteAsync(long id)
    {
        _confirmDeleteId = null;
        try
        {
            await Api.DeleteNoteAsync(id);
            _notes.RemoveAll(n => n.Id == id);
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
        catch { }
    }

    private static string FormatTime(int seconds)
        => TimeSpan.FromSeconds(seconds).ToString(seconds >= 3600 ? @"h\:mm\:ss" : @"m\:ss");
}
