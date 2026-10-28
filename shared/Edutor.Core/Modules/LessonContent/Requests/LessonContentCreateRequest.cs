using Edutor.Core.Modules.LessonContent.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Core.Modules.LessonContent.Requests;

public class LessonContentCreateRequest
{
    /// <summary>Yaradan istifadəçi — controller tərəfindən doldurulur (admin üçün null).</summary>
    public long? UserId { get; set; }

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

    /// <summary>
    /// Fayl sonra bağlanacaq: material dərhal yaranır, böyük video fonda yüklənib
    /// <c>attach</c> ucu ilə əlavə olunur. Bu halda fayl tələb olunmur.
    /// </summary>
    [FromForm(Name = "defer_file")]
    public bool DeferFile { get; set; }

    /// <summary>Materialın görünəcəyi əlavə dərslər.</summary>
    [FromForm(Name = "lesson_ids")]
    public List<long> LessonIds { get; set; } = new();

    /// <summary>Əlavə dərslər üzrə sıra: JSON {"lessonId": sortOrder}.</summary>
    [FromForm(Name = "lesson_sorts")]
    public string? LessonSorts { get; set; }
}
