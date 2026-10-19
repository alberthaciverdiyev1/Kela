using Microsoft.AspNetCore.Mvc;

using Kela.Core.Common.Requests;

namespace Kela.Core.Modules.Quiz.Requests;

public class QuizListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "lesson_id")]
    public long? LessonId { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    /// <summary>Sahibinə görə filtr (teacher yalnız öz imtahanlarını görür).</summary>
    [FromQuery(Name = "user_id")]
    public long? UserId { get; set; }
}
