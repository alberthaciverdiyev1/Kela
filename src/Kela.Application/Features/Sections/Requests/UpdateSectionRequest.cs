namespace Kela.Application.Features.Sections.Requests;

/// <summary>Mevcut sınıfı güncelleme isteği.</summary>
public sealed record UpdateSectionRequest(string Name, int Level, int? TeacherId);
