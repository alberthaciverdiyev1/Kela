using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Banner.Requests;

public class BannerUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }
}
