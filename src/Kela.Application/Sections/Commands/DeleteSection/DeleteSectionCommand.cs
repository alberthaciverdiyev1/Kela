using Kela.Application.Abstractions.Cqrs;

namespace Kela.Application.Sections.Commands.DeleteSection;

public sealed record DeleteSectionCommand(int Id) : ICommand;
