using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Edutor.Core.Modules.Group.Requests;
using Microsoft.AspNetCore.Http;

namespace Edutor.Core.Modules.Group.Validators;

public sealed class GroupUpsertRequestValidator : AbstractValidator<GroupUpsertRequest>
{
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public GroupUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(L["Validation_NameRequired"])
            .MaximumLength(190).WithMessage(L["Validation_NameMaxLength190"]);

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage(L["Validation_DescriptionMaxLength255"]);

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
