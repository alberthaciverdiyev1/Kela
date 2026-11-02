namespace Edutor.App.Shared.Dtos.StudentNote;

public record StudentNoteDto(
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

public record StudentNoteUpsertDto(
    long? ContentId,
    int? PositionSeconds,
    string? Title,
    string Body,
    string Visibility,
    long? GroupId,
    string? Color = null,
    bool? IsPinned = null);
