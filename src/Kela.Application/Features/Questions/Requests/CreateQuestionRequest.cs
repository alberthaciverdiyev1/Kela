namespace Kela.Application.Features.Questions.Requests;

public sealed record CreateQuestionRequest(
    int TeacherId,
    string Text,
    string OptionA,
    string OptionB,
    string OptionC,
    string? OptionD,
    string? OptionE,
    int CorrectOption);
