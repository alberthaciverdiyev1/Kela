using FluentValidation;
using Kela.Application.Features.Students.Requests;

namespace Kela.Application.Features.Students.Validators;

internal sealed class CreateStudentValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ad zorunludur.");

        RuleFor(x => x.LastName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Soyad zorunludur.");

        RuleFor(x => x.PhoneNumber)
            .Must(p => string.IsNullOrWhiteSpace(p) || p.Trim().Length <= 20)
            .WithMessage("Telefon en fazla 20 karakter olabilir.");

        RuleFor(x => x.BirthDate)
            .Must(b => b is null || b <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Doğum tarihi gelecekte olamaz.");
    }
}
