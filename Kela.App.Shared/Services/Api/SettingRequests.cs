using Kela.App.Shared.Dtos;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Settings ---
    public Task<ContactDto> ContactAsync() => _api.GetAsync<ContactDto>("/api/setting/contact");
}
