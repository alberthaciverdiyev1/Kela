using FluentValidation;
using Kela.Core.Modules.Group.Requests;
using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Group.Validators;

public sealed class GroupUpsertRequestValidator : AbstractValidator<GroupUpsertRequest>
{
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public GroupUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(190).WithMessage("Name may not be longer than 190 characters");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description may not be longer than 255 characters");

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
