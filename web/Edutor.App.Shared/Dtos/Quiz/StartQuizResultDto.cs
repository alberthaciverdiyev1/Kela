namespace Edutor.App.Shared.Dtos.Quiz;

public record StartQuizResultDto(long AttemptId, string Title, int TotalQuestions, List<QuizQuestionDto> Questions, int DurationMinutes = 0);
