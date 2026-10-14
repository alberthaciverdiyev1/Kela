using Microsoft.AspNetCore.Http;
using FluentValidation;
using Kela.Core.Modules.Banner.Requests;

namespace Kela.Core.Modules.Banner.Validators;

public sealed class BannerUpsertRequestValidator : AbstractValidator<BannerUpsertRequest>
{
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public BannerUpsertRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(190).WithMessage("Title may not be longer than 190 characters");

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
