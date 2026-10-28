using FluentValidation;
using Edutor.Core.Common.Localization;
using Edutor.Core.Common.Validation;
using Edutor.Core.Modules.User.Requests;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.User.Validators;

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
