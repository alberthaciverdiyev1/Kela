using Kela.Application.Pagination;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Cities;

public interface ICityRepository
{
    Task<City?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<City>> GetPageAsync(int page, CancellationToken cancellationToken = default);

    void Add(City city);
    void Update(City city);
    void Remove(City city);
}
