using FluentValidation;
using LanguageLMS.Core.Modules.Certificate.Requests;

namespace LanguageLMS.Core.Modules.Certificate.Validators;

public sealed class CertificateUpsertRequestValidator : AbstractValidator<CertificateUpsertRequest>
{
    public CertificateUpsertRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlıq tələb olunur")
            .MaximumLength(190);

        RuleFor(x => x.TemplateKey)
            .NotEmpty().WithMessage("Şablon seçilməlidir")
            .Must(CertificateTemplates.Exists).WithMessage("Şablon mövcud deyil");

        RuleFor(x => x.BodyText)
            .NotEmpty().WithMessage("Mətn tələb olunur")
            .MaximumLength(2000);
    }
}
