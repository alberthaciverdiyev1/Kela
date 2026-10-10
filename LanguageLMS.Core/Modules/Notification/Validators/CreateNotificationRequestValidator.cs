using FluentValidation;
using LanguageLMS.Core.Modules.Notification.Requests;

namespace LanguageLMS.Core.Modules.Notification.Validators;

public sealed class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(190).WithMessage("Title may not be longer than 190 characters");

        RuleFor(x => x.StudentIds)
            .Must(ids => ids.Count > 0)
            .WithMessage("At least one student is required when not sending to all")
            .When(x => !x.All);
    }
}
