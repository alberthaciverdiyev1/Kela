using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Edutor.Core.Modules.Question.Requests;

namespace Edutor.Core.Modules.Question.Validators;

public sealed class QuestionUpsertRequestValidator : AbstractValidator<QuestionUpsertRequest>
{
    private static readonly string[] Allowed = ["A", "B", "C", "D", "E"];

    public QuestionUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.SubjectId).GreaterThan(0).WithMessage(L["Validation_SubjectRequired"]);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .When(x => x.TitleType == "text");
        RuleFor(x => x.Title).MaximumLength(1000);

        RuleFor(x => x.TitleType).Must(t => t is "text" or "image").WithMessage(L["Validation_TitleType"]);
        RuleFor(x => x.VariantType).Must(t => t is "text" or "image").WithMessage(L["Validation_VariantType"]);

        RuleFor(x => x.VariantA).NotEmpty().WithMessage(L["Validation_VariantARequired"]).When(x => x.VariantType == "text");
        RuleFor(x => x.VariantB).NotEmpty().WithMessage(L["Validation_VariantBRequired"]).When(x => x.VariantType == "text");
        RuleFor(x => x.VariantC).NotEmpty().WithMessage(L["Validation_VariantCRequired"]).When(x => x.VariantType == "text");
        RuleFor(x => x.VariantD).NotEmpty().WithMessage(L["Validation_VariantDRequired"]).When(x => x.VariantType == "text");

        RuleFor(x => x.CorrectAnswer)
            .Must(a => Allowed.Contains(a?.ToUpperInvariant()))
            .WithMessage(L["Validation_CorrectAnswer"]);
    }
}
