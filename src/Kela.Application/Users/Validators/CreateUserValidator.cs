using Kela.Application.Users.Requests;
using Kela.Application.Validation;

namespace Kela.Application.Users.Validators;

internal sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    protected override void ValidateCore(CreateUserRequest value, Dictionary<string, string> errors)
    {
        if (string.IsNullOrWhiteSpace(value.FirstName))
        {
            AddError(errors, nameof(CreateUserRequest.FirstName), "Ad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(value.LastName))
        {
            AddError(errors, nameof(CreateUserRequest.LastName), "Soyad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(value.Email))
        {
            AddError(errors, nameof(CreateUserRequest.Email), "E-posta zorunludur.");
        }
        else if (!IsValidEmail(value.Email))
        {
            AddError(errors, nameof(CreateUserRequest.Email), "Geçerli bir e-posta adresi girin.");
        }

        if (string.IsNullOrWhiteSpace(value.Password))
        {
            AddError(errors, nameof(CreateUserRequest.Password), "Şifre zorunludur.");
        }
        else if (value.Password.Length < 6)
        {
            AddError(errors, nameof(CreateUserRequest.Password), "Şifre en az 6 karakter olmalıdır.");
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
