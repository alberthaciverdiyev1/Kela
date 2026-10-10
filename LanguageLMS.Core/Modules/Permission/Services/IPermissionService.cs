using LanguageLMS.Core.Modules.Permission.Requests;
using LanguageLMS.Core.Modules.Permission.Responses;

namespace LanguageLMS.Core.Modules.Permission.Services;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionResponse>> ListAsync(CancellationToken ct = default);

    Task<PermissionResponse> CreateAsync(CreatePermissionRequest request, CancellationToken ct = default);

    Task SetRolePermissionsAsync(SetRolePermissionsRequest request, CancellationToken ct = default);

    /// <summary>Verilmiş rolün həmin icazəsi var. admin həmişə true; rol üçün heç bir icazə təyin olunmayıbsa → true (permissive).</summary>
    Task<bool> HasAsync(string role, string permissionName, CancellationToken ct = default);
}
