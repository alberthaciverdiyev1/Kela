namespace Kela.Application.Features.Lessons.Responses;

public sealed record LessonResponse(
    int ContentId,
    int TeacherId,
    string Title,
    string? Description,
    string? VideoPath,
    string? ThumbnailPath,
    int DurationSeconds,
    bool IsPublished,
    int OrderIndex);
