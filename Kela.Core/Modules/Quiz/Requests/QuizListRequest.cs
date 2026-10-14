using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Quiz.Requests;

public class QuizListRequest
{
    [FromQuery(Name = "lesson_id")]
    public long? LessonId { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;
}
