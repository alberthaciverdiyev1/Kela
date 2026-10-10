using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Banner.Responses;

namespace LanguageLMS.Mvc.Models.Banner;

public class BannerIndexViewModel
{
    public string? Search { get; set; }

    public PagedResponse<BannerResponse> Page { get; set; } = default!;
}
