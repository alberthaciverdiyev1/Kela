using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.User.Requests;
using Edutor.Core.Modules.User.Responses;

namespace Edutor.Core.Modules.User.Abstractions;

public interface IUserService
{
    Task<PagedResponse<UserResponse>> ListAsync(UserListRequest request, CancellationToken ct = default);

    Task<UserResponse> GetAsync(long id, CancellationToken ct = default);

    Task<UserResponse> CreateAsync(UserUpsertRequest request, CancellationToken ct = default);

    Task<UserResponse> UpdateAsync(long id, UserUpsertRequest request, CancellationToken ct = default);

    /// <summary>İstifadəçinin öz profilini (ad, soyad, email, telefon) yeniləməsi.</summary>
    Task<UserResponse> UpdateProfileAsync(long id, UserProfileRequest request, CancellationToken ct = default);

    /// <summary>İstifadəçinin öz şifrəsini dəyişməsi (cari şifrə yoxlanılır).</summary>
    Task ChangePasswordAsync(long id, UserChangePasswordRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task<List<string>> GetSidebarHiddenAsync(long id, CancellationToken ct = default);

    Task<List<string>> SetSidebarHiddenAsync(long id, IReadOnlyCollection<string> keys, CancellationToken ct = default);
}
