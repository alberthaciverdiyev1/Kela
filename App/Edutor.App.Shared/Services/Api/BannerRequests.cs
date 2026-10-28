using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;

namespace Edutor.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Banners ---
    public Task<List<BannerDto>> BannersAsync() => _api.GetAsync<List<BannerDto>>("/api/banner");
}
