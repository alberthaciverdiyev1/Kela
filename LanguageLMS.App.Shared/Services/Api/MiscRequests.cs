using LanguageLMS.App.Shared.Dtos;

namespace LanguageLMS.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Misc ---
    public Task<List<PrivacyPolicyDto>> PrivacyPolicyAsync() => _api.GetAsync<List<PrivacyPolicyDto>>("/api/privacy-policy");
}
