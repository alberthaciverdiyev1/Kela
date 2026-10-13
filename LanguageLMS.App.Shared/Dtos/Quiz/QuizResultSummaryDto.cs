namespace LanguageLMS.App.Shared.Dtos.Quiz;

public record QuizResultSummaryDto(
    long AttemptId,
    string QuizTitle,
    int CorrectCount,
    int TotalQuestions,
    int ScorePercent,
    DateTimeOffset? StartDate,
    int? DurationSeconds);
