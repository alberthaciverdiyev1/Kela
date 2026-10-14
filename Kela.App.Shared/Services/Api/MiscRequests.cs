using Kela.App.Shared.Dtos;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Misc ---
    public Task<List<PrivacyPolicyDto>> PrivacyPolicyAsync() => _api.GetAsync<List<PrivacyPolicyDto>>("/api/privacy-policy");
}
