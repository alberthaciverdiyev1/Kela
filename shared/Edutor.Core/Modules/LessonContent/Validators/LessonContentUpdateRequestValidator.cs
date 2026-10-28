using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Edutor.Core.Modules.LessonContent.Requests;

namespace Edutor.Core.Modules.LessonContent.Validators;

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
