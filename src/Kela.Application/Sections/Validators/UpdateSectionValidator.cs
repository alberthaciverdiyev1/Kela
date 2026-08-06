using Kela.Application.Sections.Requests;
using Kela.Application.Validation;

namespace Kela.Application.Sections.Validators;

internal sealed class UpdateSectionValidator : Validator<UpdateSectionRequest>
{
    protected override void ValidateCore(UpdateSectionRequest value, Dictionary<string, string> errors)
    {
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            AddError(errors, nameof(UpdateSectionRequest.Name), "Sınıf adı zorunludur.");
        }
        else if (value.Name.Trim().Length < 2)
        {
            AddError(errors, nameof(UpdateSectionRequest.Name), "Sınıf adı en az 2 karakter olmalıdır.");
        }

        if (value.Level < 1)
        {
            AddError(errors, nameof(UpdateSectionRequest.Level), "Level 1 veya daha büyük olmalıdır.");
        }
    }
}
