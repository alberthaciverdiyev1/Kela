using Edutor.App.Shared.Dtos.StudentNote;

namespace Edutor.App.Shared.Services.Api;

public sealed partial class AppApi
{
    public Task<List<StudentNoteDto>> NotesAsync(
        long? contentId = null, long? lessonId = null, string? scope = null, bool? onlyFree = null)
    {
        var query = new List<string>();
        if (contentId is not null) query.Add($"content_id={contentId}");
        if (lessonId is not null) query.Add($"lesson_id={lessonId}");
        if (!string.IsNullOrWhiteSpace(scope)) query.Add($"scope={Uri.EscapeDataString(scope)}");
        if (onlyFree is true) query.Add("only_free=true");

        var path = "/api/student-note" + (query.Count == 0 ? "" : "?" + string.Join("&", query));
        return _api.GetAsync<List<StudentNoteDto>>(path);
    }

    public Task<StudentNoteDto> CreateNoteAsync(StudentNoteUpsertDto dto)
        => _api.PostAsync<StudentNoteDto>("/api/student-note", dto);

    public Task<StudentNoteDto> UpdateNoteAsync(long id, StudentNoteUpsertDto dto)
        => _api.PutAsync<StudentNoteDto>($"/api/student-note/{id}", dto);

    public Task<string> DeleteNoteAsync(long id)
        => _api.DeleteAsync<string>($"/api/student-note/{id}");

    public Task<StudentNoteDto> PinNoteAsync(long id, bool pinned)
        => _api.PutAsync<StudentNoteDto>($"/api/student-note/{id}/pin", new { pinned });

    public Task<StudentNoteDto> SetNoteColorAsync(long id, string color)
        => _api.PutAsync<StudentNoteDto>($"/api/student-note/{id}/color", new { color });
}
