using Kela.Application.Pagination;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Users;

/// <summary>
/// Kullanıcı okuma sorguları. Yazma (create/update/delete, parola, rol)
/// ASP.NET Core Identity'nin UserManager/RoleManager'ı ile yürütülür.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<User>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
