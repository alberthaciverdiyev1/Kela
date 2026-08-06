using FluentValidation;
using Kela.Application.Features.Sections.Requests;

namespace Kela.Application.Features.Sections.Validators;

internal sealed class CreateSectionValidator : AbstractValidator<CreateSectionRequest>
{
    public CreateSectionValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Sınıf adı zorunludur.")
            .Must(x => x.Trim().Length >= 2).WithMessage("Sınıf adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Level)
            .GreaterThanOrEqualTo(1).WithMessage("Level 1 veya daha büyük olmalıdır.");
    }
}
