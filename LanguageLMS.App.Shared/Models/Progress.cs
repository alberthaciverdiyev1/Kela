namespace LanguageLMS.App.Shared.Models;

public record ProgressContentRef(long ContentId, string Title);

public record ProgressItem(long LessonId, string LessonTitle, int TotalContents, int CompletedContents,
    int ProgressPercent, ProgressContentRef? NextContent);

public record ProgressOverview(string Type, List<ProgressItem> Items);

public record StatisticsOverview(int TotalLessons, int TotalContents, int CompletedContents, int CompletionPercent);

public record SubjectStat(long SubjectId, string SubjectName, int TotalContents, int CompletedContents, int CompletionPercent);

public record Statistics(StatisticsOverview Overview, List<SubjectStat> BySubject);
