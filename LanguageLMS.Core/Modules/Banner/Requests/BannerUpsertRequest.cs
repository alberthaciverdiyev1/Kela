using Microsoft.AspNetCore.Http;

namespace LanguageLMS.Core.Modules.Banner.Requests;

public class BannerUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }
}
