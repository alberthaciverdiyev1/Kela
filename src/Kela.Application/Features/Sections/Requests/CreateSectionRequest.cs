namespace Kela.Application.Features.Sections.Requests;

/// <summary>Yeni sınıf oluşturma isteği.</summary>
public sealed record CreateSectionRequest(string Name, int Level, int? TeacherId);
