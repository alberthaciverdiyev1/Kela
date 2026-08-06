using FluentValidation;
using Kela.Application.Features.Students.Requests;

namespace Kela.Application.Features.Students.Validators;

internal sealed class CreateStudentValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentValidator()
    {
        // Ad ve telefon zorunlu; soyad ve e-posta isteğe bağlı.
        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ad zorunludur.");

        RuleFor(x => x.PhoneNumber)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Telefon zorunludur.")
            .MaximumLength(20).WithMessage("Telefon en fazla 20 karakter olabilir.");

        RuleFor(x => x.LastName)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Trim().Length <= 100)
            .WithMessage("Soyad en fazla 100 karakter olabilir.");

        // E-posta opsiyoneldir — boşsa sistem otomatik üretir.
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Geçerli bir e-posta adresi girin.");

        RuleFor(x => x.BirthDate)
            .Must(b => b is null || b <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Doğum tarihi gelecekte olamaz.");
    }
}
