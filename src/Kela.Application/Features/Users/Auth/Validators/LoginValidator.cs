using FluentValidation;
using Kela.Application.Features.Users.Auth.Requests;

namespace Kela.Application.Features.Users.Auth.Validators;

internal sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("E-posta zorunludur.");

        RuleFor(x => x.Password)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Şifre zorunludur.");
    }
}
