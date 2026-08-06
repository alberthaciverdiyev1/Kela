using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Validation;

namespace Kela.Application.Features.Users.Auth.Validators;

internal sealed class LoginValidator : Validator<LoginRequest>
{
    protected override void ValidateCore(LoginRequest value, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value.Email))
        {
            AddError(errors, "E-posta zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(value.Password))
        {
            AddError(errors, "Şifre zorunludur.");
        }
    }
}
