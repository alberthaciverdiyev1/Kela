using Kela.Application.Abstractions.Cqrs;

namespace Kela.Application.Grades.Commands.UpdateGrade;

public sealed record UpdateGradeCommand(
    int Id,
    string Name,
    int Level,
    int? TeacherId) : ICommand;
