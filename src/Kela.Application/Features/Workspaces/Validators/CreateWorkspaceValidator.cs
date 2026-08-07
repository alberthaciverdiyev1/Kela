using FluentValidation;
using Kela.Application.Features.Workspaces.Requests;

namespace Kela.Application.Features.Workspaces.Validators;

internal sealed class CreateWorkspaceValidator : AbstractValidator<CreateWorkspaceRequest>
{
    public CreateWorkspaceValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("İş alanı adı zorunludur.")
            .Must(x => x.Trim().Length >= 2).WithMessage("İş alanı adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.TeacherId)
            .GreaterThan(0).WithMessage("Geçerli bir öğretmen gerekir.");
    }
}
