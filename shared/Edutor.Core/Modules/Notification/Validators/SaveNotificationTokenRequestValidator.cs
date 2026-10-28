using FluentValidation;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Notification.Requests;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.Notification.Validators;

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
