using Kela.Application.Users.Auth.Requests;
using Kela.Application.Validation;

namespace Kela.Application.Users.Auth.Validators;

internal sealed class LoginValidator : Validator<LoginRequest>
{
    protected override void ValidateCore(LoginRequest value, Dictionary<string, string> errors)
    {
        if (string.IsNullOrWhiteSpace(value.Email))
        {
            AddError(errors, nameof(LoginRequest.Email), "E-posta zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(value.Password))
        {
            AddError(errors, nameof(LoginRequest.Password), "Şifre zorunludur.");
        }
    }
}
