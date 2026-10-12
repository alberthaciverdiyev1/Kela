using LanguageLMS.App.Shared.Models;

namespace LanguageLMS.App.Shared.Services;

public sealed partial class AppApi
{
    // --- Banners ---
    public Task<List<BannerResponse>> BannersAsync() => _api.GetAsync<List<BannerResponse>>("/api/banner");
}
