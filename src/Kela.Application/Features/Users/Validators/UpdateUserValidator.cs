using Kela.Application.Features.Users.Requests;
using Kela.Application.Validation;

namespace Kela.Application.Features.Users.Validators;

internal sealed class UpdateUserValidator : Validator<UpdateUserRequest>
{
    protected override void ValidateCore(UpdateUserRequest value, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value.FirstName))
        {
            AddError(errors, "Ad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(value.LastName))
        {
            AddError(errors, "Soyad zorunludur.");
        }

        if (!string.IsNullOrWhiteSpace(value.Password) && value.Password.Length < 6)
        {
            AddError(errors, "Şifre en az 6 karakter olmalıdır.");
        }
    }
}
