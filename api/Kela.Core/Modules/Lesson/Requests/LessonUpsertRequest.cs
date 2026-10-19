using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Lesson.Requests;

public class LessonUpsertRequest
{
    /// <summary>Yaradan istifadəçi (teacher yalnız öz qeydlərini görür).</summary>
    public long? UserId { get; set; }

    [FromForm(Name = "subject_id")]
    public long SubjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }

    [FromForm(Name = "sort_order")]
    public int SortOrder { get; set; }

    [FromForm(Name = "is_active")]
    public bool IsActive { get; set; } = true;

    [FromForm(Name = "settings")]
    public string? Settings { get; set; }
}
