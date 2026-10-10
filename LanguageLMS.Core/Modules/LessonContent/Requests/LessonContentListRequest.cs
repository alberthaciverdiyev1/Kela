using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.LessonContent.Requests;

public class LessonContentListRequest
{
    [FromQuery(Name = "lesson_id")]
    public long? LessonId { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 50;
}
