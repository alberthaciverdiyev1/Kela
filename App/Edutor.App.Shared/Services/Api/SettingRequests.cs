using Edutor.App.Shared.Dtos;

namespace Edutor.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Settings ---
    public Task<ContactDto> ContactAsync() => _api.GetAsync<ContactDto>("/api/setting/contact");

    /// <summary>Platforma (superadmin) əlaqə məlumatları.</summary>
    public Task<ContactDto> PlatformContactAsync() => _api.GetAsync<ContactDto>("/api/setting/platform");
}
