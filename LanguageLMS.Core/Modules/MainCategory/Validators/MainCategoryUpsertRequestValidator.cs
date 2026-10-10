using FluentValidation;
using LanguageLMS.Core.Modules.MainCategory.Requests;

namespace LanguageLMS.Core.Modules.MainCategory.Validators;

public sealed class MainCategoryUpsertRequestValidator : AbstractValidator<MainCategoryUpsertRequest>
{
    public MainCategoryUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(190).WithMessage("Name may not be longer than 190 characters");
    }
}
