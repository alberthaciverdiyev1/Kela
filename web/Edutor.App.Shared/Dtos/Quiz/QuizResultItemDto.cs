namespace Edutor.App.Shared.Dtos.Quiz;

public record QuizResultItemDto(
    long QuestionId,
    string Title,
    string? VariantA,
    string? VariantB,
    string? VariantC,
    string? VariantD,
    string? VariantE,
    string? StudentAnswer,
    string CorrectAnswer,
    bool IsRight);
