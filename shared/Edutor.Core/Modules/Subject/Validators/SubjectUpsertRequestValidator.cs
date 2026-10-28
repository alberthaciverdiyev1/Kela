using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Edutor.Core.Modules.Subject.Requests;

namespace Edutor.Core.Modules.Subject.Validators;

public sealed class SubjectUpsertRequestValidator : AbstractValidator<SubjectUpsertRequest>
{
    public SubjectUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(L["Validation_NameRequired"])
            .MaximumLength(190).WithMessage(L["Validation_NameMaxLength190"]);
    }
}
