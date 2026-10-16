using Microsoft.AspNetCore.Mvc;

using Kela.Core.Common.Requests;

namespace Kela.Core.Modules.Banner.Requests;

public class BannerListRequest : ListRequestBase
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }
}
