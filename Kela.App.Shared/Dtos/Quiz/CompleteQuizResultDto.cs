namespace Kela.App.Shared.Dtos.Quiz;

public record CompleteQuizResultDto(long AttemptId, int TotalQuestions, int CorrectCount, int ScorePercent);
