using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Common.Validation;

/// <summary>Şifrə güc siyasəti: ən az 8 simvol, ən azı 1 hərf və 1 rəqəm.</summary>
public static class PasswordRules
{
    public const int MinLength = 8;

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(
        this IRuleBuilder<T, string> rule, IStringLocalizer<SharedResource> L)
        => rule
            .NotEmpty().WithMessage(L["Validation_PasswordRequired"])
            .MinimumLength(MinLength).WithMessage(L["Validation_PasswordMinLength"])
            .Matches("[A-Za-z]").WithMessage(L["Validation_PasswordLetter"])
            .Matches("[0-9]").WithMessage(L["Validation_PasswordDigit"]);
}
