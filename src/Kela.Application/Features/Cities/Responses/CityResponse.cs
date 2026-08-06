namespace Kela.Application.Features.Cities.Responses;

public sealed record CityResponse(
    int Id,
    string Language,
    string Name,
    IReadOnlyDictionary<string, string> Translations,
    DateTime CreatedAt);
