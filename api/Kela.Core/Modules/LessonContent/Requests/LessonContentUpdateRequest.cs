using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.LessonContent.Requests;

public class LessonContentUpdateRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [FromForm(Name = "sort_order")]
    public int SortOrder { get; set; }

    [FromForm(Name = "is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>Materialın görünəcəyi əlavə dərslər (əsas dərs dəyişmir).</summary>
    [FromForm(Name = "lesson_ids")]
    public List<long> LessonIds { get; set; } = new();

    /// <summary>Əlavə dərslər üzrə sıra: JSON {"lessonId": sortOrder}.</summary>
    [FromForm(Name = "lesson_sorts")]
    public string? LessonSorts { get; set; }
}
