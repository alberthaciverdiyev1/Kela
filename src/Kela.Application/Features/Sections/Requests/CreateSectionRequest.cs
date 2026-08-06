namespace Kela.Application.Features.Sections.Requests;

public sealed record CreateSectionRequest(string Name, int Level, int? TeacherId);
