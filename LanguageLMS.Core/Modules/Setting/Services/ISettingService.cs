using LanguageLMS.Core.Modules.Setting.Requests;
using LanguageLMS.Core.Modules.Setting.Responses;

namespace LanguageLMS.Core.Modules.Setting.Services;

public interface ISettingService
{
    Task<SettingResponse> GetAsync(CancellationToken ct = default);

    Task<SettingResponse> UpdateAsync(SettingUpsertRequest request, CancellationToken ct = default);

    Task<AppVersionResponse> GetAppVersionAsync(CancellationToken ct = default);
}
