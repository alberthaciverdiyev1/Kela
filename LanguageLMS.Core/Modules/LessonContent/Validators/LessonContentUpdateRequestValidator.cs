using FluentValidation;
using LanguageLMS.Core.Modules.LessonContent.Requests;

namespace LanguageLMS.Core.Modules.LessonContent.Validators;

public sealed class LessonContentUpdateRequestValidator : AbstractValidator<LessonContentUpdateRequest>
{
    public LessonContentUpdateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(190).WithMessage("Title may not be longer than 190 characters");

        RuleFor(x => x.Description).MaximumLength(500);
    }
}
