using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Group.Requests;

public class GroupListRequest
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    [FromQuery(Name = "user_id")]
    public long? UserId { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;
}
