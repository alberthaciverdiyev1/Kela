using Kela.Application.Features.Sections.Requests;
using Kela.Application.Validation;

namespace Kela.Application.Features.Sections.Validators;

internal sealed class UpdateSectionValidator : Validator<UpdateSectionRequest>
{
    protected override void ValidateCore(UpdateSectionRequest value, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            AddError(errors, "Sınıf adı zorunludur.");
        }
        else if (value.Name.Trim().Length < 2)
        {
            AddError(errors, "Sınıf adı en az 2 karakter olmalıdır.");
        }

        if (value.Level < 1)
        {
            AddError(errors, "Level 1 veya daha büyük olmalıdır.");
        }
    }
}
