using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Kela.Core.Common.Validation;
using Kela.Core.Modules.User.Entities;
using Kela.Core.Modules.User.Requests;

namespace Kela.Core.Modules.User.Validators;

public sealed class UserUpsertRequestValidator : AbstractValidator<UserUpsertRequest>
{
    public UserUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(L["Validation_EmailRequired"])
            .EmailAddress().WithMessage(L["Validation_EmailInvalid"]);

        RuleFor(x => x.Type)
            .Must(UserTypes.IsValid).WithMessage(L["Validation_UserType"]);

        RuleFor(x => x.Password)
            .MinimumLength(PasswordRules.MinLength).WithMessage(L["Validation_PasswordMinLength"])
            .Matches("[A-Za-z]").WithMessage(L["Validation_PasswordLetter"])
            .Matches("[0-9]").WithMessage(L["Validation_PasswordDigit"])
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x.Name).MaximumLength(190);
        RuleFor(x => x.Surname).MaximumLength(190);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
    }
}
