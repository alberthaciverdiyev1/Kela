using Microsoft.AspNetCore.Mvc;

using Edutor.Core.Common.Requests;

namespace Edutor.Core.Modules.LessonContent.Requests;

public class LessonContentListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "lesson_id")]
    public long? LessonId { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    /// <summary>Dərsin sahibinə görə filtr (teacher yalnız öz materiallarını görür).</summary>
    [FromQuery(Name = "user_id")]
    public long? UserId { get; set; }
}
