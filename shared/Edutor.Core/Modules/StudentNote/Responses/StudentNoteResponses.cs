namespace Edutor.Core.Modules.StudentNote.Responses;

public record StudentNoteResponse(
    long Id,
    bool IsMine,
    string AuthorName,
    long? LessonId,
    string? LessonTitle,
    long? ContentId,
    string? ContentTitle,
    int? PositionSeconds,
    string? Title,
    string Body,
    string Color,
    bool IsPinned,
    string Visibility,
    long? GroupId,
    string? GroupName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
