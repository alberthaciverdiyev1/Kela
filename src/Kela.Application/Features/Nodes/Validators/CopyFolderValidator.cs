using FluentValidation;
using Kela.Application.Features.Nodes.Requests;

namespace Kela.Application.Features.Nodes.Validators;

internal sealed class CopyFolderValidator : AbstractValidator<CopyFolderRequest>
{
    public CopyFolderValidator()
    {
        RuleFor(x => x.WorkspaceId).GreaterThan(0).WithMessage("Geçerli bir iş alanı gerekir.");
        RuleFor(x => x.SourceNodeId).GreaterThan(0).WithMessage("Geçerli bir kaynak klasör gerekir.");
    }
}
