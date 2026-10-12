namespace LanguageLMS.App.Shared.Dtos.Progress;

public record SubjectStatDto(long SubjectId, string SubjectName, int TotalContents, int CompletedContents, int CompletionPercent);
