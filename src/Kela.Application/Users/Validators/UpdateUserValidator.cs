using Kela.Application.Users.Requests;
using Kela.Application.Validation;

namespace Kela.Application.Users.Validators;

internal sealed class UpdateUserValidator : Validator<UpdateUserRequest>
{
    protected override void ValidateCore(UpdateUserRequest value, Dictionary<string, string> errors)
    {
        if (string.IsNullOrWhiteSpace(value.FirstName))
        {
            AddError(errors, nameof(UpdateUserRequest.FirstName), "Ad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(value.LastName))
        {
            AddError(errors, nameof(UpdateUserRequest.LastName), "Soyad zorunludur.");
        }

        if (!string.IsNullOrWhiteSpace(value.Password) && value.Password.Length < 6)
        {
            AddError(errors, nameof(UpdateUserRequest.Password), "Şifre en az 6 karakter olmalıdır.");
        }
    }
}
