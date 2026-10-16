using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Banner.Requests;

public class BannerUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }

    [FromForm(Name = "is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>Boş = müddətsiz. Format: yyyy-MM-dd.</summary>
    [FromForm(Name = "active_until")]
    public DateOnly? ActiveUntil { get; set; }
}
