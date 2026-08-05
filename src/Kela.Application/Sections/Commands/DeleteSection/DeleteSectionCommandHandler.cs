using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;

namespace Kela.Application.Sections.Commands.DeleteSection;

internal sealed class DeleteSectionCommandHandler(ISectionRepository sections, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteSectionCommand>
{
    public async Task Handle(DeleteSectionCommand command, CancellationToken cancellationToken)
    {
        var section = await sections.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {command.Id} olan sınıf bulunamadı.");

        sections.Remove(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
