using FluentValidation;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Homework.Requests;
using Edutor.Core.Modules.Homework.Services;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.Homework.Validators;

public sealed class HomeworkUpsertRequestValidator : AbstractValidator<HomeworkUpsertRequest>
{
    public HomeworkUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190).WithMessage(L["Validation_TitleMaxLength190"]);

        // Rich editor HTML saxlayır — limit mətn limitindən böyükdür.
        RuleFor(x => x.Description).MaximumLength(20000);

        RuleFor(x => x.GroupDueDates)
            .Must((request, dates) => dates.Count == request.GroupIds.Count)
            .WithMessage(L["Homework_GroupDeadlineRequired"]);

        RuleFor(x => x.GroupScores)
            .Must((request, scores) => scores.Count == request.GroupIds.Count)
            .WithMessage(L["Homework_GroupDeadlineRequired"]);

        RuleForEach(x => x.GroupDueDates)
            .Must(date => date != default).WithMessage(L["Homework_GroupDeadlineRequired"]);

        RuleForEach(x => x.GroupScores)
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
