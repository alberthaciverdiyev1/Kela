using FluentValidation;
using LanguageLMS.Core.Modules.Question.Requests;

namespace LanguageLMS.Core.Modules.Question.Validators;

public sealed class QuestionUpsertRequestValidator : AbstractValidator<QuestionUpsertRequest>
{
    private static readonly string[] Allowed = ["A", "B", "C", "D", "E"];

    public QuestionUpsertRequestValidator()
    {
        RuleFor(x => x.SubjectId).GreaterThan(0).WithMessage("Subject is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(1000);

        RuleFor(x => x.VariantA).NotEmpty().WithMessage("Variant A is required");
        RuleFor(x => x.VariantB).NotEmpty().WithMessage("Variant B is required");
        RuleFor(x => x.VariantC).NotEmpty().WithMessage("Variant C is required");
        RuleFor(x => x.VariantD).NotEmpty().WithMessage("Variant D is required");

        RuleFor(x => x.CorrectAnswer)
            .Must(a => Allowed.Contains(a?.ToUpperInvariant()))
            .WithMessage("Correct answer must be one of A, B, C, D, E");
    }
}
