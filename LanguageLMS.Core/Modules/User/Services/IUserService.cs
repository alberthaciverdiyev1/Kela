using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.User.Requests;
using LanguageLMS.Core.Modules.User.Responses;

namespace LanguageLMS.Core.Modules.User.Services;

public interface IUserService
{
    Task<PagedResponse<UserResponse>> ListAsync(UserListRequest request, CancellationToken ct = default);

    Task<UserResponse> GetAsync(long id, CancellationToken ct = default);

    Task<UserResponse> CreateAsync(UserUpsertRequest request, CancellationToken ct = default);

    Task<UserResponse> UpdateAsync(long id, UserUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
