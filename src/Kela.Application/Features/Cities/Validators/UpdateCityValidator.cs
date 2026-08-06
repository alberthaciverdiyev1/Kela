using FluentValidation;
using Kela.Application.Features.Cities.Requests;
using Kela.Domain.Common;

namespace Kela.Application.Features.Cities.Validators;

internal sealed class UpdateCityValidator : AbstractValidator<UpdateCityRequest>
{
    public UpdateCityValidator()
    {
        RuleFor(x => x.Translations)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Şehir adları (Translations) zorunludur.")
            .Must(ContainsAllLanguages)
                .WithMessage($"Şehir adları şu dillerin tümünde verilmelidir: {string.Join(", ", LanguageCodes.All)}.")
            .Must(AllNamesValid)
                .WithMessage("Her dildeki şehir adı zorunlu ve en az 2 karakter olmalıdır.");
    }

    private static bool ContainsAllLanguages(Dictionary<string, string>? translations)
        => translations is not null && LanguageCodes.All.All(translations.ContainsKey);

    private static bool AllNamesValid(Dictionary<string, string>? translations)
        => translations is not null
           && translations.Values.All(v => !string.IsNullOrWhiteSpace(v) && v.Trim().Length >= 2);
}
