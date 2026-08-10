using Kela.Application.Features.Questions.Responses;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Questions;

public static class QuestionMappings
{
    public static QuestionResponse ToResponse(this Question question) => new(
        question.Id,
        question.TeacherId,
        question.Text,
        question.OptionA,
        question.OptionB,
        question.OptionC,
        question.OptionD,
        question.OptionE,
        question.CorrectOption,
        question.CreatedAt);
}
