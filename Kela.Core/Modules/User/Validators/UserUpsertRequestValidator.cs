using FluentValidation;
using Kela.Core.Modules.User.Entities;
using Kela.Core.Modules.User.Requests;

namespace Kela.Core.Modules.User.Validators;

public sealed class UserUpsertRequestValidator : AbstractValidator<UserUpsertRequest>
{
    public UserUpsertRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(x => x.Type)
            .Must(UserTypes.IsValid).WithMessage("Type must be 'admin' or 'teacher'");

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x.Name).MaximumLength(190);
        RuleFor(x => x.Surname).MaximumLength(190);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
    }
}
