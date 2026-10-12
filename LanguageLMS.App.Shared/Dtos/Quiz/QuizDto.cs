namespace LanguageLMS.App.Shared.Dtos.Quiz;

public record QuizDto(long Id, string Title, int DurationMinutes, int QuestionCount, string Type);
