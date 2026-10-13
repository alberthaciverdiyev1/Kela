using LanguageLMS.App.Shared.Dtos;

namespace LanguageLMS.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Settings ---
    public Task<ContactDto> ContactAsync() => _api.GetAsync<ContactDto>("/api/setting/contact");
}
