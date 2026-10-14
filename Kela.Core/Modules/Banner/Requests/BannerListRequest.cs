using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Banner.Requests;

public class BannerListRequest
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;
}
