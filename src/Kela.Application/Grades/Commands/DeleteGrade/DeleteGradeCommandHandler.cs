using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;

namespace Kela.Application.Grades.Commands.DeleteGrade;

internal sealed class DeleteGradeCommandHandler(IGradeRepository grades, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteGradeCommand>
{
    public async Task Handle(DeleteGradeCommand command, CancellationToken cancellationToken)
    {
        var grade = await grades.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {command.Id} olan sınıf bulunamadı.");

        grades.Remove(grade);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
