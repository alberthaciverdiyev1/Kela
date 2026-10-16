using Microsoft.AspNetCore.Mvc;

using Kela.Core.Common.Requests;

namespace Kela.Core.Modules.Lesson.Requests;

public class LessonListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "subject_id")]
    public long? SubjectId { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }
}
