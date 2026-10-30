using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.TeacherNote.Requests;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.TeacherNote.Validators;

public sealed class TeacherNoteUpsertRequestValidator : AbstractValidator<TeacherNoteUpsertRequest>
{
    public TeacherNoteUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190).WithMessage(L["Validation_TitleTooLong"]);

        RuleFor(x => x.Body).MaximumLength(2000).WithMessage(L["Validation_TextTooLong"]);
    }
}
