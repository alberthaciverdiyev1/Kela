using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Validation;

namespace Kela.Application.Features.Users.Auth.Validators;

internal sealed class RegisterValidator : Validator<RegisterRequest>
{
    protected override void ValidateCore(RegisterRequest value, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value.FirstName))
        {
            AddError(errors, "Ad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(value.LastName))
        {
            AddError(errors, "Soyad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(value.Email))
        {
            AddError(errors, "E-posta zorunludur.");
        }
        else if (!IsValidEmail(value.Email))
        {
            AddError(errors, "Geçerli bir e-posta adresi girin.");
        }

        if (string.IsNullOrWhiteSpace(value.Password))
        {
            AddError(errors, "Şifre zorunludur.");
        }
        else if (value.Password.Length < 6)
        {
            AddError(errors, "Şifre en az 6 karakter olmalıdır.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            return address.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
