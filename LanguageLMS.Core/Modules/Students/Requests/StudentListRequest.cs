using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.Students.Requests;

public class StudentListRequest
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_active")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "group_id")]
    public long? GroupId { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;
}

public class SetStudentStatusRequest
{
    public bool IsActive { get; set; }
}
