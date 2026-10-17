using Microsoft.AspNetCore.Mvc;

using Kela.Core.Common.Requests;

namespace Kela.Core.Modules.Students.Requests;

public class StudentListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "group_id")]
    public long? GroupId { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }
}

public class SetStudentStatusRequest
{
    public bool IsActive { get; set; }
}
