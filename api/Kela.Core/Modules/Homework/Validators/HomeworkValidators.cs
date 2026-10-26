using FluentValidation;
using Kela.Core.Common.Localization;
using Kela.Core.Modules.Homework.Requests;
using Kela.Core.Modules.Homework.Services;
using Microsoft.Extensions.Localization;

namespace Kela.Core.Modules.Homework.Validators;

public sealed class HomeworkUpsertRequestValidator : AbstractValidator<HomeworkUpsertRequest>
{
    public HomeworkUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190).WithMessage(L["Validation_TitleMaxLength190"]);

        // Rich editor HTML saxlayır — limit mətn limitindən böyükdür.
        RuleFor(x => x.Description).MaximumLength(20000);

        RuleFor(x => x.MaxScore)
            .GreaterThan(0).WithMessage(L["Homework_MaxScoreInvalid"])
            .LessThanOrEqualTo(1000).WithMessage(L["Homework_MaxScoreInvalid"]);

        RuleForEach(x => x.GroupIds).GreaterThan(0);

        RuleForEach(x => x.QuizIds).GreaterThan(0);
    }
}

public sealed class SubmitHomeworkRequestValidator : AbstractValidator<SubmitHomeworkRequest>
{
    public SubmitHomeworkRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Text)
            .MaximumLength(10000).WithMessage(L["Homework_TextTooLong"]);

        RuleFor(x => x.Link)
            .MaximumLength(500)
            .Must(link => string.IsNullOrWhiteSpace(link) || HomeworkFileRules.IsHttpUrl(link))
            .WithMessage(L["Homework_LinkInvalid"]);

        RuleFor(x => x.Files)
            .Must(files => files is null || files.Count <= HomeworkFileRules.MaxFiles)
            .WithMessage(L["Homework_TooManyFiles"]);

        RuleForEach(x => x.Files)
            .Must(file => file.Length <= HomeworkFileRules.MaxFileBytes)
            .WithMessage(L["Homework_FileTooLarge"]);
    }
}

public sealed class GradeHomeworkRequestValidator : AbstractValidator<GradeHomeworkRequest>
{
    public GradeHomeworkRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Feedback).MaximumLength(4000);
    }
}
