using FluentValidation;
using Kela.Core.Common.Localization;
using Microsoft.Extensions.Localization;
using Kela.Core.Common.Validation;
using Kela.Core.Modules.Auth.Requests;

namespace Kela.Core.Modules.Auth.Validators;

public sealed class SendOtpRequestValidator : AbstractValidator<SendOtpRequest>
{
    public SendOtpRequestValidator(IStringLocalizer<SharedResource> L)
        => RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(L["Validation_EmailInvalid"]);
}

public sealed class CheckOtpRequestValidator : AbstractValidator<CheckOtpRequest>
{
    public CheckOtpRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(L["Validation_EmailInvalid"]);
        RuleFor(x => x.Otp).NotEmpty().WithMessage(L["Validation_OtpRequired"]);
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(L["Validation_EmailInvalid"]);
        RuleFor(x => x.Otp).NotEmpty().WithMessage(L["Validation_OtpRequired"]);
        RuleFor(x => x.NewPassword).StrongPassword(L);
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(L["Validation_CurrentPasswordRequired"]);
        RuleFor(x => x.NewPassword).StrongPassword(L);
    }
}

public sealed class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator(IStringLocalizer<SharedResource> L)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(L["Validation_EmailInvalid"]);
        RuleFor(x => x.Otp).NotEmpty().WithMessage(L["Validation_OtpRequired"]);
    }
}

public sealed class ChangeFullNameRequestValidator : AbstractValidator<ChangeFullNameRequest>
{
    public ChangeFullNameRequestValidator(IStringLocalizer<SharedResource> L)
        => RuleFor(x => x.FullName).NotEmpty().MaximumLength(255).WithMessage(L["Validation_FullNameRequired"]);
}

public sealed class ChangePhoneRequestValidator : AbstractValidator<ChangePhoneRequest>
{
    public ChangePhoneRequestValidator(IStringLocalizer<SharedResource> L)
        => RuleFor(x => x.Phone).NotEmpty().MaximumLength(50).WithMessage(L["Validation_PhoneRequired"]);
}
