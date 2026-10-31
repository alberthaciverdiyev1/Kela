namespace Edutor.Core.Modules.TeacherNote.Responses;

public record TeacherNoteUserResponse(long Id, string Name, string Type);

public record TeacherNoteContentOption(long Id, string Title, string LessonTitle, string? Type);

public record TeacherNoteResponse(
    long Id,
    bool IsMine,
    string AuthorName,
    string Title,
    string? Body,
    DateOnly? Date,
    bool IsDone,
    long? ContentId,
    string? ContentTitle,
    long? LessonId,
    string? LessonTitle,
    int? PositionSeconds,
    string Visibility,
    IReadOnlyList<TeacherNoteUserResponse> SharedWith,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
