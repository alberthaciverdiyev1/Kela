using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Edutor.Core.Modules.Certificate.Requests;

namespace Edutor.Core.Modules.Certificate.Validators;

public sealed class CertificateUpsertRequestValidator : AbstractValidator<CertificateUpsertRequest>
{
    public CertificateUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(L["Validation_TitleRequired"])
            .MaximumLength(190);

        RuleFor(x => x.TemplateKey)
            .NotEmpty().WithMessage(L["Validation_TemplateRequired"])
            .Must(CertificateTemplates.Exists).WithMessage(L["Validation_TemplateNotExists"]);

        RuleFor(x => x.BodyText)
            .NotEmpty().WithMessage(L["Validation_BodyTextRequired"])
            .MaximumLength(2000);
    }
}
