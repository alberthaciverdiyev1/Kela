using FluentValidation;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Attendance.Requests;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.Attendance.Validators;

public sealed class SaveAttendanceRequestValidator : AbstractValidator<SaveAttendanceRequest>
{
    public SaveAttendanceRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.GroupId).GreaterThan(0).WithMessage(L["Validation_GroupRequired"]);
        RuleFor(x => x.Date).NotEmpty().WithMessage(L["Validation_DateRequired"]);
        RuleFor(x => x.Students).NotNull().WithMessage(L["Validation_StudentsRequired"]);
        RuleForEach(x => x.Students).ChildRules(item =>
            item.RuleFor(x => x.Point)
                .InclusiveBetween(0, 100)
                .When(x => x.Point.HasValue)
                .WithMessage(L["Attendance_PointInvalid"]));
    }
}
