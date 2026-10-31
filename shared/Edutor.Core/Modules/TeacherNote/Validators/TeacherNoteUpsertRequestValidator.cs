using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.TeacherNote.Entities;
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

        RuleFor(x => x.Visibility)
            .Must(v => v is TeacherNoteVisibility.Private or TeacherNoteVisibility.Shared)
            .WithMessage(L["Note_InvalidVisibility"]);

        RuleFor(x => x.SharedWithUserIds)
            .NotEmpty().WithMessage(L["Note_ShareTargetRequired"])
            .When(x => x.Visibility == TeacherNoteVisibility.Shared);

        RuleFor(x => x.PositionSeconds)
            .GreaterThanOrEqualTo(0).WithMessage(L["Note_InvalidPosition"])
            .When(x => x.PositionSeconds is not null);
    }
}
