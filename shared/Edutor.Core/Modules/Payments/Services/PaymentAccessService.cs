using Edutor.Core.Data;
using Edutor.Core.Modules.TenantConfig.Abstractions;
using Edutor.Core.Modules.TenantConfig.Entities;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Payments.Services;

public sealed class PaymentAccessService(AppDbContext db, ITenantConfigService config) : IPaymentAccessService
{
    public async Task<PaymentAccessState> GetAsync(long studentId, CancellationToken ct = default)
    {
        var settings = await config.GetAsync(ct);
        if (settings.PaymentBlockMode == PaymentBlockModes.None) return PaymentAccessState.Allow;

        // Plan təyin edilməyibsə gecikmə anlayışı yoxdur — bloklanmır.
        var plan = await db.StudentPaymentPlans.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.Id)
            .Select(x => new { x.NextDueOn })
            .FirstOrDefaultAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (plan is null || plan.NextDueOn >= today) return PaymentAccessState.Allow;

        return settings.PaymentBlockMode == PaymentBlockModes.All
            ? new PaymentAccessState(true, [])
            : new PaymentAccessState(false, settings.PaymentBlockedGroupIds);
    }
}
