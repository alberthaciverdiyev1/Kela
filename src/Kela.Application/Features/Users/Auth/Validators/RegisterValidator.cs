using FluentValidation;
using Kela.Application.Features.Users.Auth.Requests;

namespace Kela.Application.Features.Users.Auth.Validators;

internal sealed class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ad zorunludur.");

        RuleFor(x => x.LastName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Soyad zorunludur.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Şifre zorunludur.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
    }
}
