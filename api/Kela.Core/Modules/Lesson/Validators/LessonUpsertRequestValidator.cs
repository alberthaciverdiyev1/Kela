using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Kela.Core.Modules.Lesson.Requests;
using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Lesson.Validators;

public sealed class LessonUpsertRequestValidator : AbstractValidator<LessonUpsertRequest>
{
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public LessonUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190).WithMessage(L["Validation_TitleMaxLength190"]);

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage(L["Validation_SubjectRequired"]);

        RuleFor(x => x.Image)
            .Must(file => file is null || file.Length <= MaxImageBytes)
            .WithMessage(L["Validation_ImageMaxSize"])
            .Must(file => file is null || IsAllowedImage(file))
            .WithMessage(L["Validation_ImageFormat"]);
    }

    private static bool IsAllowedImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png";
    }
}
