using Edutor.Core.Modules.Subscription.Responses;

namespace Edutor.Admin.Models;

public class PlatformDashboardModel
{
    public PlatformFinanceResponse Finance { get; set; } = default!;

    public string UserName { get; set; } = string.Empty;
}
