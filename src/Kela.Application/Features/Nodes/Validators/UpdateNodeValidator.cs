using FluentValidation;
using Kela.Application.Features.Nodes.Requests;

namespace Kela.Application.Features.Nodes.Validators;

internal sealed class UpdateNodeValidator : AbstractValidator<UpdateNodeRequest>
{
    public UpdateNodeValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Name) || x.ParentId is not null || x.Position is not null)
            .WithMessage("Güncellenecek bir alan belirtilmelidir.");
    }
}
