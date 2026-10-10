using LanguageLMS.Core.Modules.LessonContent.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.LessonContent.Requests;

public class LessonContentCreateRequest
{
    [FromForm(Name = "lesson_id")]
    public long LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [FromForm(Name = "type")]
    public LessonContentType Type { get; set; }

    [FromForm(Name = "sort_order")]
    public int SortOrder { get; set; }

    [FromForm(Name = "is_active")]
    public bool IsActive { get; set; } = true;

    public IFormFile? File { get; set; }

    /// <summary>Birbaşa R2-yə (presigned) yüklənmiş faylın yolu. File verilməzsə istifadə olunur.</summary>
    public string? Path { get; set; }
}
