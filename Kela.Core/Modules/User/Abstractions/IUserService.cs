using Kela.Core.Common.Responses;
using Kela.Core.Modules.User.Requests;
using Kela.Core.Modules.User.Responses;

namespace Kela.Core.Modules.User.Abstractions;

public interface IUserService
{
    Task<PagedResponse<UserResponse>> ListAsync(UserListRequest request, CancellationToken ct = default);

    Task<UserResponse> GetAsync(long id, CancellationToken ct = default);

    Task<UserResponse> CreateAsync(UserUpsertRequest request, CancellationToken ct = default);

    Task<UserResponse> UpdateAsync(long id, UserUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
