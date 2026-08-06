namespace Kela.Application.Sections.Requests;

/// <summary>Yeni sınıf oluşturma isteği.</summary>
public sealed record CreateSectionRequest(string Name, int Level, int? TeacherId);
