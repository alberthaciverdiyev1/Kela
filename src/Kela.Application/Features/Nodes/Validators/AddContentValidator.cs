using FluentValidation;
using Kela.Application.Features.Nodes.Requests;

namespace Kela.Application.Features.Nodes.Validators;

internal sealed class AddContentValidator : AbstractValidator<AddContentRequest>
{
    public AddContentValidator()
    {
        RuleFor(x => x.WorkspaceId).GreaterThan(0).WithMessage("Geçerli bir iş alanı gerekir.");
        RuleFor(x => x.ContentId).GreaterThan(0).WithMessage("Geçerli bir içerik gerekir.");
    }
}
