using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Kela.Core.Modules.Quiz.Requests;

namespace Kela.Core.Modules.Quiz.Validators;

public sealed class QuizUpsertRequestValidator : AbstractValidator<QuizUpsertRequest>
{
    public QuizUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.LessonId)
            .NotNull().GreaterThan(0)
            .WithMessage(L["Validation_LessonRequired"])
            .When(x => x.Type == "lesson");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190).WithMessage(L["Validation_TitleMaxLength190"]);

        // 0 dəqiqə mənasızdır — imtahanın minimum müddəti 1 dəqiqədir.
        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(1).WithMessage(L["Quiz_DurationInvalid"])
            .LessThanOrEqualTo(600).WithMessage(L["Quiz_DurationInvalid"]);
    }
}
