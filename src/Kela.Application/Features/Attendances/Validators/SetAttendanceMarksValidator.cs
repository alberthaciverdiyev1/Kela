using FluentValidation;
using Kela.Application.Features.Attendances.Requests;

namespace Kela.Application.Features.Attendances.Validators;

internal sealed class SetAttendanceMarksValidator : AbstractValidator<SetAttendanceMarksRequest>
{
    public SetAttendanceMarksValidator()
    {
        RuleFor(x => x.Marks)
            .NotNull().WithMessage("Yoklama işaretleri zorunludur.")
            .Must(marks => marks is null || marks.Select(m => m.StudentId).Distinct().Count() == marks.Count)
            .WithMessage("Aynı öğrenci için birden fazla işaret gönderilemez.");

        RuleForEach(x => x.Marks).ChildRules(mark =>
        {
            mark.RuleFor(m => m.StudentId)
                .GreaterThan(0).WithMessage("Geçerli bir öğrenci gerekir.");
            mark.RuleFor(m => m.Status)
                .IsInEnum().WithMessage("Geçersiz yoklama durumu.");
            mark.RuleFor(m => m.Note)
                .MaximumLength(200).WithMessage("Not en fazla 200 karakter olabilir.");
        });
    }
}
