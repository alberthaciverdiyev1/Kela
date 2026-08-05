using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Grades.Commands.UpdateGrade;

internal sealed class UpdateGradeCommandHandler(
    IGradeRepository grades,
    IUserRepository users,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateGradeCommand>
{
    public async Task Handle(UpdateGradeCommand command, CancellationToken cancellationToken)
    {
        var grade = await grades.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {command.Id} olan sınıf bulunamadı.");

        var name = command.Name.Trim();
        if (name != grade.Name && await grades.NameExistsAsync(name, cancellationToken))
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

        grade.Name = name;
        grade.Level = command.Level;
        grade.TeacherId = command.TeacherId;
        grade.UpdatedAt = DateTime.UtcNow;

        grades.Update(grade);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
