using FluentValidation;
using Kela.Core.Common.Localization;
using Kela.Core.Modules.Notification.Requests;
using Microsoft.Extensions.Localization;

namespace Kela.Core.Modules.Notification.Validators;

public sealed class SaveNotificationTokenRequestValidator : AbstractValidator<SaveNotificationTokenRequest>
{
    public SaveNotificationTokenRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(L["Validation_TokenRequired"])
            .MaximumLength(500).WithMessage(L["Validation_TokenMaxLength"]);

        RuleFor(x => x.DeviceType)
            .MaximumLength(20).WithMessage(L["Validation_DeviceTypeMaxLength"]);
    }
}
