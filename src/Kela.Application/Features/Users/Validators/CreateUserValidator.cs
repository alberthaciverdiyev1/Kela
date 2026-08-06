using FluentValidation;
using Kela.Application.Features.Users.Requests;
using Kela.Domain.Common;

namespace Kela.Application.Features.Users.Validators;

internal sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ad zorunludur.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Şifre zorunludur.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");

        // Rol, Identity rol adlarından biri olmalıdır (RoleNames).
        RuleFor(x => x.Role)
            .Must(RoleNames.IsValid)
            .WithMessage($"Rol şunlardan biri olmalıdır: {string.Join(", ", RoleNames.All)}.");
    }
}
