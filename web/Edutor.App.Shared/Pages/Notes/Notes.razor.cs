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

    private bool _loading = true;
    private List<StudentNoteDto> _notes = new();
    private List<GroupDto> _groups = new();

    private string _scope = "";
    private bool _formOpen;
    private long? _editingId;
    private string _title = "";
    private string _body = "";
    private string _visibility = "private";
    private long? _groupId;
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

    private void StartCreate()
    {
        ResetForm();
        _formOpen = true;
    }

    private void StartEdit(StudentNoteDto note)
    {
        _editingId = note.Id;
        _title = note.Title ?? "";
        _body = note.Body;
        _visibility = note.Visibility;
        _groupId = note.GroupId;
        _formContentId = note.ContentId;
        _formPosition = note.PositionSeconds;
        _error = null;
        _formOpen = true;
    }

    private void CancelForm()
    {
        ResetForm();
        _formOpen = false;
    }

    private void ResetForm()
    {
        _editingId = null;
        _title = "";
        _body = "";
        _visibility = "private";
        _groupId = _groups.FirstOrDefault()?.Id;
        _formContentId = null;
        _formPosition = null;
        _error = null;
    }

    private void SetVisibilityAsync(string visibility)
    {
        _visibility = visibility;
        if (visibility == "group" && _groupId is null) _groupId = _groups.FirstOrDefault()?.Id;
    }

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
            _visibility == "group" ? _groupId : null);

        try
        {
            if (_editingId is long id) await Api.UpdateNoteAsync(id, dto);
            else await Api.CreateNoteAsync(dto);

            _formOpen = false;
            ResetForm();
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
