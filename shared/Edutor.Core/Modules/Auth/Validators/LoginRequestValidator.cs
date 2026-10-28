using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Edutor.Core.Modules.Auth.Requests;

namespace Edutor.Core.Modules.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(L["Validation_EmailRequired"])
            .EmailAddress().WithMessage(L["Validation_EmailInvalid"]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(L["Validation_PasswordRequired"])
            .MinimumLength(6).WithMessage(L["Validation_PasswordMinLength"]);
    }
}
