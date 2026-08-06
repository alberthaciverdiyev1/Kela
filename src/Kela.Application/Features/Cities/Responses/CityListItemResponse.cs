namespace Kela.Application.Features.Cities.Responses;

/// <summary>Şehir listesi öğesi — istenen dile göre yerelleştirilmiş ad.</summary>
public sealed record CityListItemResponse(
    int Id,
    string Language,
    string Name,
    DateTime CreatedAt);
