namespace LanguageLMS.Core.Modules.LessonContent.Responses;

public record LessonContentResponse(
    long Id,
    long LessonId,
    string Title,
    string? Description,
    string? Type,
    string? Url,
    int SortOrder,
    bool IsActive,
    int? DurationSeconds);
