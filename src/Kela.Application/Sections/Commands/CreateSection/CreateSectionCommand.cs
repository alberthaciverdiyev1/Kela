using System.ComponentModel.DataAnnotations;
using Kela.Application.Abstractions.Cqrs;

namespace Kela.Application.Sections.Commands.CreateSection;

public sealed record CreateSectionCommand(
    [property: Required, StringLength(50)] string Name,
    [property: Range(1, 12)] int Level,
    int? TeacherId) : ICommand<int>;
