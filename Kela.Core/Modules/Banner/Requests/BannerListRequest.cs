using Microsoft.AspNetCore.Mvc;

using Kela.Core.Common.Requests;

namespace Kela.Core.Modules.Banner.Requests;

public class BannerListRequest : ListRequestBase
{
    /// <summary>Yalnız aktiv və müddəti bitməmiş bannerlər (şagird app-i üçün).</summary>
    public bool OnlyActive { get; set; }

    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }
}
