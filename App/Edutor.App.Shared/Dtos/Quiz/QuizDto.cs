namespace Edutor.App.Shared.Dtos.Quiz;

public record QuizDto(long Id, string Title, int DurationMinutes, int QuestionCount, string Type, long? LessonId = null, string? LessonTitle = null);
