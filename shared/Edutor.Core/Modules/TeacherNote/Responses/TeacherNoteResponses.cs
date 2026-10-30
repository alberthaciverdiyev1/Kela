namespace Edutor.Core.Modules.TeacherNote.Responses;

public record TeacherNoteResponse(
    long Id,
    string Title,
    string? Body,
    DateOnly? Date,
    bool IsDone,
    DateTimeOffset CreatedAt);
