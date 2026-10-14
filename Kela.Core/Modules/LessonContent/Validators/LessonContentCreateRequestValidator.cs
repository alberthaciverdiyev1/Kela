using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Kela.Core.Modules.LessonContent.Requests;

namespace Kela.Core.Modules.LessonContent.Validators;

public sealed class LessonContentCreateRequestValidator : AbstractValidator<LessonContentCreateRequest>
{
    public LessonContentCreateRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.LessonId).GreaterThan(0).WithMessage(L["Validation_LessonRequired"]);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190).WithMessage(L["Validation_TitleMaxLength190"]);

        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x)
            .Must(x => x.File is not null || !string.IsNullOrWhiteSpace(x.Path))
            .WithMessage(L["Validation_FileOrPathRequired"]);
    }
}
