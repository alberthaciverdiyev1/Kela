using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;
using Kela.Domain.Sections;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Sections.Commands.CreateSection;

internal sealed class CreateSectionCommandHandler(
    ISectionRepository sections,
    IUserRepository users,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSectionCommand, int>
{
    public async Task<int> Handle(CreateSectionCommand command, CancellationToken cancellationToken)
    {
        var name = command.Name.Trim();

        if (await sections.NameExistsAsync(name, cancellationToken))
        {
            throw new InvalidOperationException($"'{name}' adlı sınıf zaten kayıtlı.");
        }

        if (command.TeacherId is int teacherId)
        {
            await EnsureTeacherExistsAsync(teacherId, cancellationToken);
        }

        var section = new Section
        {
            Name = name,
            Level = command.Level,
            TeacherId = command.TeacherId,
            CreatedAt = DateTime.UtcNow,
        };

        sections.Add(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return section.Id;
    }

    private async Task EnsureTeacherExistsAsync(int teacherId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(teacherId, cancellationToken);
        if (user is null || user.Role != Role.Teacher)
        {
            throw new InvalidOperationException($"Id = {teacherId} olan öğretmen bulunamadı.");
        }
    }
}
