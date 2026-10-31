using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.StudentNote.Requests;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.StudentNote.Validators;

public sealed class StudentNoteUpsertRequestValidator : AbstractValidator<StudentNoteUpsertRequest>
{
    public StudentNoteUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage(L["Validation_BodyTextRequired"])
            .MaximumLength(4000);

        RuleFor(x => x.Title).MaximumLength(190).WithMessage(L["Validation_TitleMaxLength190"]);

        RuleFor(x => x.PositionSeconds).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Visibility)
            .Must(v => v is "private" or "group")
            .WithMessage(L["StudentNote_InvalidVisibility"]);
    }
}
