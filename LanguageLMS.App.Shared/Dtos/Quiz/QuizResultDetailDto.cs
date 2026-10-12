namespace LanguageLMS.App.Shared.Dtos.Quiz;

public record QuizResultDetailDto(long AttemptId, long QuizId, string QuizTitle, int TotalQuestions, int CorrectCount,
    int ScorePercent, bool IsCompleted, List<QuizResultItemDto> Items);
