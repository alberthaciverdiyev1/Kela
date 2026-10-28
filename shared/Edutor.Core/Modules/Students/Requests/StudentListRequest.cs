using Microsoft.AspNetCore.Mvc;

using Edutor.Core.Common.Requests;

namespace Edutor.Core.Modules.Students.Requests;

public class StudentListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "group_id")]
    public long? GroupId { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    /// <summary>Teacher scope: yalnız həmin müəllimin qruplarındaki şagirdlər.</summary>
    [FromQuery(Name = "owner_id")]
    public long? OwnerId { get; set; }
}

public class SetStudentStatusRequest
{
    public bool IsActive { get; set; }
}
