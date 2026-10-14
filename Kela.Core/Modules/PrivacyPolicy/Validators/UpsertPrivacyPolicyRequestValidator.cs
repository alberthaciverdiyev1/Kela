using FluentValidation;
using Kela.Core.Modules.PrivacyPolicy.Requests;

namespace Kela.Core.Modules.PrivacyPolicy.Validators;

public sealed class UpsertPrivacyPolicyRequestValidator : AbstractValidator<UpsertPrivacyPolicyRequest>
{
    public UpsertPrivacyPolicyRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .MaximumLength(50);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");
    }
}
