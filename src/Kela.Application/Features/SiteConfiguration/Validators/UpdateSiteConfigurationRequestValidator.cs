using System.Text.RegularExpressions;
using FluentValidation;
using Kela.Application.Features.SiteConfiguration.Requests;

namespace Kela.Application.Features.SiteConfiguration.Validators;

internal sealed class UpdateSiteConfigurationRequestValidator : AbstractValidator<UpdateSiteConfigurationRequest>
{
    private static readonly Regex HexColor = new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

    public UpdateSiteConfigurationRequestValidator()
    {
        RuleFor(x => x.SiteName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Site adı zorunludur.")
            .MaximumLength(50).WithMessage("Site adı en fazla 50 karakter olmalıdır.");

        RuleFor(x => x.PrimaryColor).Must(BeValidHex).WithMessage("Ana renk geçerli bir hex olmalıdır (#RRGGBB).");
        RuleFor(x => x.SecondaryColor).Must(BeValidHex).WithMessage("İkincil renk geçerli bir hex olmalıdır (#RRGGBB).");
        RuleFor(x => x.SuccessColor).Must(BeValidHex).WithMessage("Başarı rengi geçerli bir hex olmalıdır (#RRGGBB).");
        RuleFor(x => x.WarningColor).Must(BeValidHex).WithMessage("Uyarı rengi geçerli bir hex olmalıdır (#RRGGBB).");
        RuleFor(x => x.ErrorColor).Must(BeValidHex).WithMessage("Hata rengi geçerli bir hex olmalıdır (#RRGGBB).");
        RuleFor(x => x.InfoColor).Must(BeValidHex).WithMessage("Bilgi rengi geçerli bir hex olmalıdır (#RRGGBB).");

        RuleFor(x => x.NavMode)
            .Must(x => x is "navbar" or "sidebar")
            .WithMessage("NavMode yalnızca 'navbar' veya 'sidebar' olabilir.");
    }

    private static bool BeValidHex(string? value)
        => !string.IsNullOrWhiteSpace(value) && HexColor.IsMatch(value);
}
