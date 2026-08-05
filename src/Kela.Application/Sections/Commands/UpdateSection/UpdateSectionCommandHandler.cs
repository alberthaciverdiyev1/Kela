using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Sections.Commands.UpdateSection;

internal sealed class UpdateSectionCommandHandler(
    ISectionRepository sections,
    IUserRepository users,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateSectionCommand>
{
    public async Task Handle(UpdateSectionCommand command, CancellationToken cancellationToken)
    {
        var section = await sections.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {command.Id} olan sınıf bulunamadı.");

        var name = command.Name.Trim();
        if (name != section.Name && await sections.NameExistsAsync(name, cancellationToken))
        {
            throw new InvalidOperationException($"'{name}' adlı sınıf zaten kayıtlı.");
        }

        if (command.TeacherId is int teacherId)
        {
            var user = await users.GetByIdAsync(teacherId, cancellationToken);
            if (user is null || user.Role != Role.Teacher)
            {
                throw new InvalidOperationException($"Id = {teacherId} olan öğretmen bulunamadı.");
            }
        }

        section.Name = name;
        section.Level = command.Level;
        section.TeacherId = command.TeacherId;
        section.UpdatedAt = DateTime.UtcNow;

        sections.Update(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
