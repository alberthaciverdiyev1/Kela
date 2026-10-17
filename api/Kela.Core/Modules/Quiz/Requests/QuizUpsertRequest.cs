using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Quiz.Requests;

public class QuizUpsertRequest
{
    [FromForm(Name = "lesson_id")]
    public long? LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    [FromForm(Name = "duration_minutes")]
    public int DurationMinutes { get; set; }

    [FromForm(Name = "type")]
    public string Type { get; set; } = "lesson";

    [FromForm(Name = "is_active")]
    public bool IsActive { get; set; } = true;

    [FromForm(Name = "is_repeat")]
    public bool IsRepeat { get; set; }

    [FromForm(Name = "sort_order")]
    public int SortOrder { get; set; }
}
