using Microsoft.AspNetCore.Http;

namespace LanguageLMS.Mvc.Models.Banner;

public class BannerFormViewModel
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }

    public string? CurrentImageUrl { get; set; }
}
