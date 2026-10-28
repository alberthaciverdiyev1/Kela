using FluentValidation;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Edutor.Core.Modules.PrivacyPolicy.Requests;

namespace Edutor.Core.Modules.PrivacyPolicy.Validators;

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
