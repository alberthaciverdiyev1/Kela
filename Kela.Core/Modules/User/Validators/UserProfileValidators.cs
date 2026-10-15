using FluentValidation;
using Kela.Core.Modules.User.Requests;

namespace Kela.Core.Modules.User.Validators;

public sealed class UserProfileRequestValidator : AbstractValidator<UserProfileRequest>
{
    public UserProfileRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).MaximumLength(190);
        RuleFor(x => x.Surname).MaximumLength(190);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
    }
}

public sealed class UserChangePasswordRequestValidator : AbstractValidator<UserChangePasswordRequest>
{
    public UserChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}
