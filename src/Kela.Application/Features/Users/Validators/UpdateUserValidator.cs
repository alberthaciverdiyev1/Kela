using FluentValidation;
using Kela.Application.Features.Users.Requests;

namespace Kela.Application.Features.Users.Validators;

internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ad zorunludur.");

        RuleFor(x => x.LastName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Soyad zorunludur.");

        // Şifre opsiyonel: girildiyse en az 6 karakter olmalı.
        When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        });
    }
}
