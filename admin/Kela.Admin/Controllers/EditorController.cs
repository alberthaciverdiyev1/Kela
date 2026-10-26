using Kela.Admin.Infrastructure;
using Kela.Core.Common.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

/// <summary>Rich editor daxilinə şəkil yükləmə (ev tapşırığı təsviri, siyasət mətni və s.).</summary>
[Route("[controller]")]
public sealed class EditorController(IFileStorage storage, ITenantFolder folders) : AdminControllerBase
{
    private const long MaxImageBytes = 5 * 1024 * 1024;

    // SVG qəsdən yoxdur — eyni origin-də skript icra edə bilər.
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    [HttpPost("image")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Image(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Şəkil seçilmədi" });

        if (file.Length > MaxImageBytes)
            return BadRequest(new { error = "Şəkil 5 MB-dan böyük ola bilməz" });

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { error = "Yalnız JPG, PNG, GIF və WEBP şəkilləri dəstəklənir" });

        var folder = $"{await folders.ResolveAsync(ct: ct)}/editor";
        var path = await storage.UploadAsync(file, folder, ct);

        return Json(new { url = storage.GetUrl(path) });
    }
}
