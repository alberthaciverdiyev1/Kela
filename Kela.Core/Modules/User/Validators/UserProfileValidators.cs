using FluentValidation;
using Kela.Core.Common.Localization;
using Kela.Core.Common.Validation;
using Kela.Core.Modules.User.Requests;
using Microsoft.Extensions.Localization;

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
    public UserChangePasswordRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).StrongPassword(L);
    }
}
