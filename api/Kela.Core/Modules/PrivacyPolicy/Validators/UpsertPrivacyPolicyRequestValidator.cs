using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Kela.Core.Modules.PrivacyPolicy.Requests;

namespace Kela.Core.Modules.PrivacyPolicy.Validators;

public sealed class UpsertPrivacyPolicyRequestValidator : AbstractValidator<UpsertPrivacyPolicyRequest>
{
    public UpsertPrivacyPolicyRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage(L["Validation_TypeRequired"])
            .MaximumLength(50);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(L["Validation_ContentRequired"]);
    }
}
