using FluentValidation;
using Kela.Core.Modules.Auth.Requests;

namespace Kela.Core.Modules.Auth.Validators;

public sealed class SendOtpRequestValidator : AbstractValidator<SendOtpRequest>
{
    public SendOtpRequestValidator()
        => RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is not valid");
}

public sealed class CheckOtpRequestValidator : AbstractValidator<CheckOtpRequest>
{
    public CheckOtpRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is not valid");
        RuleFor(x => x.Otp).NotEmpty().WithMessage("OTP is required");
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is not valid");
        RuleFor(x => x.Otp).NotEmpty().WithMessage("OTP is required");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}

public sealed class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is not valid");
        RuleFor(x => x.Otp).NotEmpty().WithMessage("OTP is required");
    }
}

public sealed class ChangeFullNameRequestValidator : AbstractValidator<ChangeFullNameRequest>
{
    public ChangeFullNameRequestValidator()
        => RuleFor(x => x.FullName).NotEmpty().MaximumLength(255).WithMessage("Full name is required");
}

public sealed class ChangePhoneRequestValidator : AbstractValidator<ChangePhoneRequest>
{
    public ChangePhoneRequestValidator()
        => RuleFor(x => x.Phone).NotEmpty().MaximumLength(50).WithMessage("Phone is required");
}
