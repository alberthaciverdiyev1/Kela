namespace LanguageLMS.App.Shared.Dtos.Progress;

public record ProgressItemDto(long LessonId, string LessonTitle, int TotalContents, int CompletedContents,
    int ProgressPercent, ProgressContentRefDto? NextContent);
