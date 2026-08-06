using Kela.Application.Features.Cities.Requests;
using Kela.Application.Features.Cities.Responses;
using Kela.Application.Pagination;

namespace Kela.Application.Features.Cities;

public interface ICityService
{
    Task<PaginatedResult<CityListItemResponse>> GetPageAsync(
        int page, int pageSize, string? language, CancellationToken cancellationToken = default);

    Task<CityResponse?> GetByIdAsync(int id, string? language, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(CreateCityRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, UpdateCityRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
