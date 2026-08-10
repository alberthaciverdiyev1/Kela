using FluentValidation;
using Kela.Application.Features.Quizzes.Requests;

namespace Kela.Application.Features.Quizzes.Validators;

internal sealed class AddQuizQuestionsValidator : AbstractValidator<AddQuizQuestionsRequest>
{
    public AddQuizQuestionsValidator()
    {
        RuleFor(x => x.QuestionIds)
            .NotNull().WithMessage("Soru listesi zorunludur.");

        RuleForEach(x => x.QuestionIds)
            .GreaterThan(0).WithMessage("Geçersiz soru kimliği.");
    }
}
