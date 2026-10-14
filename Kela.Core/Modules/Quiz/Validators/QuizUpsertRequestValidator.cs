using FluentValidation;
using Kela.Core.Modules.Quiz.Requests;

namespace Kela.Core.Modules.Quiz.Validators;

public sealed class QuizUpsertRequestValidator : AbstractValidator<QuizUpsertRequest>
{
    public QuizUpsertRequestValidator()
    {
        RuleFor(x => x.LessonId)
            .NotNull().GreaterThan(0)
            .WithMessage("Lesson is required")
            .When(x => x.Type == "lesson");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(190).WithMessage("Title may not be longer than 190 characters");

        RuleFor(x => x.DurationMinutes).GreaterThanOrEqualTo(0);
    }
}
