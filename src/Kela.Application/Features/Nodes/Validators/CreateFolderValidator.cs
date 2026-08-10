using FluentValidation;
using Kela.Application.Features.Nodes.Requests;

namespace Kela.Application.Features.Nodes.Validators;

internal sealed class CreateFolderValidator : AbstractValidator<CreateFolderRequest>
{
    public CreateFolderValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.WorkspaceId is null) != (x.TeacherId is null))
            .WithMessage("Klasör bağlamı belirtilmelidir (workspace veya kütüphane).");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Klasör adı zorunludur.")
            .Must(x => x.Trim().Length >= 1).WithMessage("Klasör adı en az 1 karakter olmalıdır.");
    }
}
