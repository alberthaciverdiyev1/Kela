using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.LessonContent.Requests;

public class LessonContentListRequest
{
    public string? Search { get; set; }

    [FromQuery(Name = "lesson_id")]
    public long? LessonId { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 50;
}
