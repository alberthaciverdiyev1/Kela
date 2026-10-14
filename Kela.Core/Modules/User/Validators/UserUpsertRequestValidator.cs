using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
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
            .MinimumLength(6).WithMessage(L["Validation_PasswordMinLength"])
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x.Name).MaximumLength(190);
        RuleFor(x => x.Surname).MaximumLength(190);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
    }
}
