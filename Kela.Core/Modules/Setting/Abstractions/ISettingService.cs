using Kela.Core.Modules.Setting.Requests;
using Kela.Core.Modules.Setting.Responses;

namespace Kela.Core.Modules.Setting.Abstractions;

public interface ISettingService
{
    Task<SettingResponse> GetAsync(CancellationToken ct = default);

    Task<SettingResponse> UpdateAsync(SettingUpsertRequest request, CancellationToken ct = default);

    /// <summary>Platforma (superadmin) əlaqə məlumatları — tenant-dan asılı olmayan, tenant 0 sətri.</summary>
    Task<SettingResponse> GetPlatformAsync(CancellationToken ct = default);

    Task<SettingResponse> UpdatePlatformAsync(SettingUpsertRequest request, CancellationToken ct = default);

    Task<AppVersionResponse> GetAppVersionAsync(CancellationToken ct = default);
}
