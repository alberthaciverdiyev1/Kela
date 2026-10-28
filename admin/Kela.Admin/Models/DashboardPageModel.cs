using Kela.Core.Modules.Dashboard.Responses;
using Kela.Core.Modules.Payments.Responses;

namespace Kela.Admin.Models;

public class DashboardPageModel
{
    public DashboardResponse Stats { get; set; } = default!;

    /// <summary>Kassa yalnız admin-lərə və fərdi müəllim tenant-ına görünür.</summary>
    public KassaOverviewResponse? Finance { get; set; }

    public string UserName { get; set; } = string.Empty;
}
