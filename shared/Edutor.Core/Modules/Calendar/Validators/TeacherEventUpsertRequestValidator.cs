using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Calendar.Requests;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.Calendar.Validators;

public sealed class TeacherEventUpsertRequestValidator : AbstractValidator<TeacherEventUpsertRequest>
{
    public TeacherEventUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190).WithMessage(L["Validation_TitleTooLong"]);

        RuleFor(x => x.Description).MaximumLength(2000).WithMessage(L["Validation_TextTooLong"]);

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt).WithMessage(L["Calendar_InvalidRange"]);
    }
}
