namespace Kela.Core.Modules.Lesson.Responses;

public record LessonResponse(
    long Id,
    long SubjectId,
    string Title,
    string? Image,
    int SortOrder,
    bool IsActive,
    string Settings);
