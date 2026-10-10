using FluentValidation;
using LanguageLMS.Core.Modules.Profession.Requests;
using Microsoft.AspNetCore.Http;

namespace LanguageLMS.Core.Modules.Profession.Validators;

public sealed class ProfessionUpsertRequestValidator : AbstractValidator<ProfessionUpsertRequest>
{
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public ProfessionUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(190).WithMessage("Name may not be longer than 190 characters");

        RuleFor(x => x.Image)
            .Must(file => file is null || file.Length <= MaxImageBytes)
            .WithMessage("Image size may not exceed 5 MB")
            .Must(file => file is null || IsAllowedImage(file))
            .WithMessage("Invalid image format (allowed: jpg, jpeg, png)");
    }

    private static bool IsAllowedImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png";
    }
}
