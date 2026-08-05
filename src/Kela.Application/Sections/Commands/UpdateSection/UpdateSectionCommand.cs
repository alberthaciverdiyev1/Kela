using System.ComponentModel.DataAnnotations;
using Kela.Application.Abstractions.Cqrs;

namespace Kela.Application.Sections.Commands.UpdateSection;

public sealed record UpdateSectionCommand(
    int Id,
    [property: Required, StringLength(50)] string Name,
    [property: Range(1, 12)] int Level,
    int? TeacherId) : ICommand;
