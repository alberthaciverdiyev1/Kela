using FluentValidation;
using Kela.Core.Modules.Question.Requests;

namespace Kela.Core.Modules.Question.Validators;

public sealed class QuestionUpsertRequestValidator : AbstractValidator<QuestionUpsertRequest>
{
    private static readonly string[] Allowed = ["A", "B", "C", "D", "E"];

    public QuestionUpsertRequestValidator()
    {
        RuleFor(x => x.SubjectId).GreaterThan(0).WithMessage("Subject is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .When(x => x.TitleType == "text");
        RuleFor(x => x.Title).MaximumLength(1000);

        RuleFor(x => x.TitleType).Must(t => t is "text" or "image").WithMessage("Title type must be text or image");
        RuleFor(x => x.VariantType).Must(t => t is "text" or "image").WithMessage("Variant type must be text or image");

        RuleFor(x => x.VariantA).NotEmpty().WithMessage("Variant A is required").When(x => x.VariantType == "text");
        RuleFor(x => x.VariantB).NotEmpty().WithMessage("Variant B is required").When(x => x.VariantType == "text");
        RuleFor(x => x.VariantC).NotEmpty().WithMessage("Variant C is required").When(x => x.VariantType == "text");
        RuleFor(x => x.VariantD).NotEmpty().WithMessage("Variant D is required").When(x => x.VariantType == "text");

        RuleFor(x => x.CorrectAnswer)
            .Must(a => Allowed.Contains(a?.ToUpperInvariant()))
            .WithMessage("Correct answer must be one of A, B, C, D, E");
    }
}
