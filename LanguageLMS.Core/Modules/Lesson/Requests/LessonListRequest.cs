using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.Lesson.Requests;

public class LessonListRequest
{
    public string? Search { get; set; }

    [FromQuery(Name = "subject_id")]
    public long? SubjectId { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;
}
