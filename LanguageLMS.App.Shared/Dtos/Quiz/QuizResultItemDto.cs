namespace LanguageLMS.App.Shared.Dtos.Quiz;

public record QuizResultItemDto(long QuestionId, string Title, string? StudentAnswer, string CorrectAnswer, bool IsRight);
