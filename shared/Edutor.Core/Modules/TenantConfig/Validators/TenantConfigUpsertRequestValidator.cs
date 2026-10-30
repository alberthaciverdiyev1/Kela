using FluentValidation;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.TenantConfig.Entities;
using Edutor.Core.Modules.TenantConfig.Requests;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.TenantConfig.Validators;

public sealed class TenantConfigUpsertRequestValidator : AbstractValidator<TenantConfigUpsertRequest>
{
    public TenantConfigUpsertRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.PaymentBlockMode)
            .Must(PaymentBlockModes.IsValid)
            .WithMessage(L["Config_BlockModeInvalid"]);

        RuleFor(x => x.PaymentBlockedGroupIds)
            .NotEmpty()
            .When(x => x.PaymentBlockMode == PaymentBlockModes.Groups)
            .WithMessage(L["Config_BlockedGroupsRequired"]);
    }
}
