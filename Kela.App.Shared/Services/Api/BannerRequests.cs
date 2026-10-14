using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Banners ---
    public Task<List<BannerDto>> BannersAsync() => _api.GetAsync<List<BannerDto>>("/api/banner");
}
