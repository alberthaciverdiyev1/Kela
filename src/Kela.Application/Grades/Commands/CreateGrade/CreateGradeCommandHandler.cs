using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;
using Kela.Domain.Grades;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Grades.Commands.CreateGrade;

internal sealed class CreateGradeCommandHandler(
    IGradeRepository grades,
    IUserRepository users,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateGradeCommand, int>
{
    public async Task<int> Handle(CreateGradeCommand command, CancellationToken cancellationToken)
    {
        var name = command.Name.Trim();

        if (await grades.NameExistsAsync(name, cancellationToken))
        {
            throw new InvalidOperationException($"'{name}' adlı sınıf zaten kayıtlı.");
        }

        if (command.TeacherId is int teacherId)
        {
            await EnsureTeacherExistsAsync(teacherId, cancellationToken);
        }

        var grade = new Grade
        {
            Id = 0, // EF identity DB'de üretir
            Name = name,
            Level = command.Level,
            TeacherId = command.TeacherId,
            CreatedAt = DateTime.UtcNow,
        };

        grades.Add(grade);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return grade.Id;
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
