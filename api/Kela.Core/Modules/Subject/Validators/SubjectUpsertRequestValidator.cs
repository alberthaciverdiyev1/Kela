using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Kela.Core.Modules.Subject.Requests;

namespace Kela.Core.Modules.Subject.Validators;

public sealed class SubjectUpsertRequestValidator : AbstractValidator<SubjectUpsertRequest>
{
    public SubjectUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(L["Validation_NameRequired"])
            .MaximumLength(190).WithMessage(L["Validation_NameMaxLength190"]);
    }
}
