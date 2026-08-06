using Kela.Application.Pagination;
using Kela.Application.Features.Users.Requests;
using Kela.Application.Features.Users.Responses;

namespace Kela.Application.Features.Users;

public interface IUserService
{
    Task<PaginatedResult<UserResponse>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
