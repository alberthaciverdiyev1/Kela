namespace Kela.Core.Modules.Progress.Responses;

public record ProgressContentRef(long ContentId, string Title, string? Type);

public record ProgressItemResponse(
    long LessonId,
    string LessonTitle,
    string? Image,
    int TotalContents,
    int CompletedContents,
    int ProgressPercent,
    ProgressContentRef? NextContent);

public record ProgressResponse(string Type, IReadOnlyList<ProgressItemResponse> Items);

public record MarkProgressResponse(long ContentId, string Status);

public record StatisticsOverview(int TotalLessons, int TotalContents, int CompletedContents, int CompletionPercent);

public record SubjectStatResponse(long SubjectId, string SubjectName, int TotalContents, int CompletedContents, int CompletionPercent);

public record StatisticsResponse(StatisticsOverview Overview, IReadOnlyList<SubjectStatResponse> BySubject);
