using Microsoft.AspNetCore.Mvc;

using Kela.Core.Common.Requests;

namespace Kela.Core.Modules.Subject.Requests;

public class SubjectListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    /// <summary>Sahibinə görə filtr: teacher öz fənnlərini və adminin yaratdıqlarını görür.</summary>
    [FromQuery(Name = "user_id")]
    public long? UserId { get; set; }
}
