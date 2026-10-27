using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Banner.Requests;

public class BannerUpsertRequest
{
    /// <summary>Yaradan istifadəçi — controller tərəfindən doldurulur (admin üçün null).</summary>
    public long? UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }

    [FromForm(Name = "is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>Boş = müddətsiz. Format: yyyy-MM-dd.</summary>
    [FromForm(Name = "active_until")]
    public DateOnly? ActiveUntil { get; set; }

    /// <summary>Görünürlük həddi — controller rola görə təyin edir (BannerScope).</summary>
    public string? Scope { get; set; }
}
