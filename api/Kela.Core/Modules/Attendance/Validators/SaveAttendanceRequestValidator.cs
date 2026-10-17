using FluentValidation;
using Kela.Core.Common.Localization;
using Kela.Core.Modules.Attendance.Requests;
using Microsoft.Extensions.Localization;

namespace Kela.Core.Modules.Attendance.Validators;

public sealed class SaveAttendanceRequestValidator : AbstractValidator<SaveAttendanceRequest>
{
    public SaveAttendanceRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.GroupId).GreaterThan(0).WithMessage(L["Validation_GroupRequired"]);
        RuleFor(x => x.Date).NotEmpty().WithMessage(L["Validation_DateRequired"]);
        RuleFor(x => x.Students).NotNull().WithMessage(L["Validation_StudentsRequired"]);
    }
}
