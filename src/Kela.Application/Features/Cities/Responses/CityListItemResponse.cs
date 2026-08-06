namespace Kela.Application.Features.Cities.Responses;

public sealed record CityListItemResponse(
    int Id,
    string Language,
    string Name,
    DateTime CreatedAt);
