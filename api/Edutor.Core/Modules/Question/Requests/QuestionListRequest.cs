using Microsoft.AspNetCore.Mvc;

using Edutor.Core.Common.Requests;

namespace Edutor.Core.Modules.Question.Requests;

public class QuestionListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "subject_id")]
    public long? SubjectId { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    /// <summary>Sahibinə görə filtr (teacher yalnız öz imtahanlarını görür).</summary>
    [FromQuery(Name = "user_id")]
    public long? UserId { get; set; }
}
