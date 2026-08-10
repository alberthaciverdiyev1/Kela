using Kela.Application.Pagination;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<User>> GetPageAsync(int page, CancellationToken cancellationToken = default);
}
