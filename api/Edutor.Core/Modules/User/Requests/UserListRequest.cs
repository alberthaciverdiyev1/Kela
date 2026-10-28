using Microsoft.AspNetCore.Mvc;

using Edutor.Core.Common.Requests;

namespace Edutor.Core.Modules.User.Requests;

public class UserListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    public string? Type { get; set; }
}
