using Kela.Application.Abstractions.Cqrs;

namespace Kela.Application.Grades.Commands.CreateGrade;

public sealed record CreateGradeCommand(
    string Name,
    int Level,
    int? TeacherId) : ICommand<int>;
