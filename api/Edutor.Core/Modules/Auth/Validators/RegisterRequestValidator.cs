using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Edutor.Core.Common.Validation;
using Edutor.Core.Modules.Auth.Requests;

namespace Edutor.Core.Modules.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(L["Validation_FullNameRequired"])
            .MaximumLength(255).WithMessage(L["Validation_FullNameMaxLength"]);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(L["Validation_EmailRequired"])
            .EmailAddress().WithMessage(L["Validation_EmailInvalid"]);

        RuleFor(x => x.Password).StrongPassword(L);
    }
}
