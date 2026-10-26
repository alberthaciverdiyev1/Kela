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

    /// <summary>Digər müəllimlərə paylaş. Yalnız sahibi (və ya admin) dəyişə bilər.</summary>
    [FromForm(Name = "is_shared")]
    public bool IsShared { get; set; }

    [FromForm(Name = "settings")]
    public string? Settings { get; set; }

    /// <summary>
    /// Yaradılan dərsi fənnin bağlı olduğu, dərs seçimi edilmiş bütün qruplara əlavə et.
    /// Yalnız yaratma zamanı istifadə olunur.
    /// </summary>
    [FromForm(Name = "add_to_groups")]
    public bool AddToGroups { get; set; }
}
