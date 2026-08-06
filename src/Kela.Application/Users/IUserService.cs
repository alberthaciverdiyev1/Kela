using Kela.Application.Pagination;
using Kela.Application.Users.Dtos;
using Kela.Domain.Enums;

namespace Kela.Application.Users;

public interface IUserService
{
    Task<PaginatedResult<UserDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(
        string firstName, string lastName, string email, string password, Role role,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(
        int id, string firstName, string lastName, string? password, UserStatus? status,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
