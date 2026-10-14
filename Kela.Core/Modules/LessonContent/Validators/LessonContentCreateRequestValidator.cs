using FluentValidation;
using Kela.Core.Modules.LessonContent.Requests;

namespace Kela.Core.Modules.LessonContent.Validators;

public sealed class LessonContentCreateRequestValidator : AbstractValidator<LessonContentCreateRequest>
{
    public LessonContentCreateRequestValidator()
    {
        RuleFor(x => x.LessonId).GreaterThan(0).WithMessage("Lesson is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(190).WithMessage("Title may not be longer than 190 characters");

        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x)
            .Must(x => x.File is not null || !string.IsNullOrWhiteSpace(x.Path))
            .WithMessage("File or Path is required");
    }
}
