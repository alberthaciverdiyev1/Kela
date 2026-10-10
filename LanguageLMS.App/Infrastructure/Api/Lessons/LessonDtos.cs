namespace LanguageLMS.App.Infrastructure.Api.Lessons;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Data,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record LessonListItem(
    long Id,
    long SubjectId,
    string Title,
    string? Image,
    int SortOrder,
    bool IsActive,
    string Settings);

public sealed record LessonListQuery(
    int Page = 1,
    int PerPage = 20,
    long? SubjectId = null,
    bool? IsActive = null,
    string? Search = null);
