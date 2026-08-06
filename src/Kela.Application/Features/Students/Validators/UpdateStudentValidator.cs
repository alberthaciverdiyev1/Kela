using FluentValidation;
using Kela.Application.Features.Students.Requests;

namespace Kela.Application.Features.Students.Validators;

internal sealed class UpdateStudentValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentValidator()
    {
        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ad zorunludur.");

        RuleFor(x => x.LastName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Soyad zorunludur.");

        RuleFor(x => x.BirthDate)
            .Must(b => b is null || b <= DateTime.UtcNow)
            .WithMessage("Doğum tarihi gelecekte olamaz.");
    }
}
