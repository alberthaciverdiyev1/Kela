using Kela.Application.Abstractions.Cqrs;

namespace Kela.Application.Grades.Commands.DeleteGrade;

public sealed record DeleteGradeCommand(int Id) : ICommand;
