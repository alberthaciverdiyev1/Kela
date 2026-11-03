namespace Edutor.App.Shared.Dtos.Progress;

public record ContinueItemDto(
    long ContentId,
    long LessonId,
    string LessonTitle,
    string Title,
    string? Type);
