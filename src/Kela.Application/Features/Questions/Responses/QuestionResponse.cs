namespace Kela.Application.Features.Questions.Responses;

public sealed record QuestionResponse(
    int Id,
    int TeacherId,
    string Text,
    string OptionA,
    string OptionB,
    string OptionC,
    string? OptionD,
    string? OptionE,
    int CorrectOption,
    DateTime CreatedAt);
