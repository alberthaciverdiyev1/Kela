using FluentValidation;
using Kela.Core.Modules.Subject.Requests;

namespace Kela.Core.Modules.Subject.Validators;

public sealed class SubjectUpsertRequestValidator : AbstractValidator<SubjectUpsertRequest>
{
    public SubjectUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(190).WithMessage("Name may not be longer than 190 characters");
    }
}
