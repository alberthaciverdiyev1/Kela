namespace Kela.Application.Features.Sections.Requests;

public sealed record UpdateSectionRequest(string Name, int Level, int? TeacherId);
