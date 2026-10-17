using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Kela.Core.Modules.LessonContent.Requests;

namespace Kela.Core.Modules.LessonContent.Validators;

public sealed class LessonContentUpdateRequestValidator : AbstractValidator<LessonContentUpdateRequest>
{
    public LessonContentUpdateRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190).WithMessage(L["Validation_TitleMaxLength190"]);

        RuleFor(x => x.Description).MaximumLength(500);
    }
}
