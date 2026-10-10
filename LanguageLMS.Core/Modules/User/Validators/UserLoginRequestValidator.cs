using FluentValidation;
using LanguageLMS.Core.Modules.User.Requests;

namespace LanguageLMS.Core.Modules.User.Validators;

public sealed class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
