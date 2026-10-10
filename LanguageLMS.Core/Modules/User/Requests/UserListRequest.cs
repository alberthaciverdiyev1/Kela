using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.User.Requests;

public class UserListRequest
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    public string? Type { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;
}
