using Edutor.Core.Data;
using Edutor.Core.Modules.Subscription.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Subscription.Services;

public sealed class PlatformFinanceService(AppDbContext db) : IPlatformFinanceService
{
    public async Task<PlatformFinanceResponse> GetAsync(int months = 12, CancellationToken ct = default)
    {
        months = Math.Clamp(months, 3, 36);

        var now = DateTimeOffset.UtcNow;
        var periods = Enumerable.Range(0, months)
            .Select(i => now.AddMonths(-(months - 1 - i)).ToString("yyyy-MM"))
            .ToList();
        var currentPeriod = periods[^1];

        var tenants = await db.Tenants.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Select(x => new
            {
                x.Id, x.Name, x.IsActive, x.IsPending, x.PlanId, x.PlanStartedAt, x.PlanExpiresAt
            })
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        var plans = await db.SubscriptionPlans.AsNoTracking()
            .Select(x => new { x.Id, x.Name, x.Currency, x.MonthlyPrice, x.SortOrder })
            .ToDictionaryAsync(x => x.Id, ct);

        var addons = await db.TenantAddons.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Select(x => new { x.TenantId, x.Period, x.Amount, x.Currency })
            .ToListAsync(ct);

        var currencies = plans.Values.Select(p => p.Currency)
            .Concat(addons.Select(a => a.Currency))
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .DefaultIfEmpty("AZN")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var priced = tenants
            .Where(t => t.PlanId is long id && plans.ContainsKey(id))
            .Select(t => (Tenant: t, Plan: plans[t.PlanId!.Value]))
            .ToList();

        bool ActiveIn(string period, DateTimeOffset? startedAt, DateTimeOffset? expiresAt)
        {
            var start = PeriodStart(period);
            var end = start.AddMonths(1);
            if (startedAt is { } started && started >= end) return false;
            if (expiresAt is { } expires && expires < start) return false;
            return true;
        }

        decimal SubscriptionsIn(string period, string currency) =>
            priced
                .Where(x => x.Plan.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)
                            && ActiveIn(period, x.Tenant.PlanStartedAt, x.Tenant.PlanExpiresAt))
                .Sum(x => x.Plan.MonthlyPrice);

        decimal AddonsIn(string period, string currency) =>
            addons
                .Where(a => a.Period == period && a.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase))
                .Sum(a => a.Amount);

        decimal RevenueAllTime(string currency)
        {
            var total = addons
                .Where(a => a.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase))
                .Sum(a => a.Amount);

            foreach (var (tenant, plan) in priced)
            {
                if (!plan.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)) continue;

                var from = tenant.PlanStartedAt ?? PeriodStart(currentPeriod);
                var to = tenant.PlanExpiresAt is { } expires && expires < now ? expires : now;
                var monthsCount = (to.Year - from.Year) * 12 + (to.Month - from.Month) + 1;
                total += plan.MonthlyPrice * Math.Max(0, monthsCount);
            }

            return total;
        }

        var lastPeriod = now.AddMonths(-1).ToString("yyyy-MM");

        var totals = currencies.Select(currency =>
        {
            decimal mrr = 0;
            var paying = 0;

            foreach (var (tenant, plan) in priced)
            {
                if (!plan.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)) continue;
                if (!tenant.IsActive || tenant.IsPending) continue;
                if (tenant.PlanExpiresAt is { } expires && expires < now) continue;

                mrr += plan.MonthlyPrice;
                paying++;
            }

            var addonsThisMonth = AddonsIn(currentPeriod, currency);

            return new PlatformCurrencyTotals(
                currency,
                paying,
                mrr,
                mrr * 12,
                SubscriptionsIn(currentPeriod, currency) + addonsThisMonth,
                SubscriptionsIn(lastPeriod, currency) + AddonsIn(lastPeriod, currency),
                addonsThisMonth,
                RevenueAllTime(currency));
        }).ToList();

        var monthly = periods
            .SelectMany(period => currencies.Select(currency => new PlatformMonthPoint(
                period, currency, SubscriptionsIn(period, currency), AddonsIn(period, currency))))
            .ToList();

        var planBreakdown = plans.Values
            .Select(plan =>
            {
                var assigned = priced.Where(x => x.Plan.Id == plan.Id).ToList();
                var paying = assigned.Count(x => x.Tenant.IsActive && !x.Tenant.IsPending
                    && (x.Tenant.PlanExpiresAt is not { } expires || expires >= now));

                return new PlatformPlanBreakdown(
                    plan.Id, plan.Name, plan.Currency, plan.MonthlyPrice, assigned.Count, plan.MonthlyPrice * paying);
            })
            .OrderBy(x => x.PlanName)
            .ToList();

        var rows = tenants.Select(tenant =>
        {
            plans.TryGetValue(tenant.PlanId ?? 0, out var plan);

            var addonThisMonth = addons
                .Where(a => a.TenantId == tenant.Id && a.Period == currentPeriod)
                .Sum(a => a.Amount);

            var paying = plan is not null && tenant.IsActive && !tenant.IsPending
                && (tenant.PlanExpiresAt is not { } expires || expires >= now);

            return new PlatformTenantRow(
                tenant.Id,
                tenant.Name,
                plan?.Name,
                plan?.MonthlyPrice ?? 0,
                plan?.Currency ?? "AZN",
                tenant.PlanStartedAt,
                tenant.PlanExpiresAt,
                Status(tenant.IsActive, tenant.IsPending, tenant.PlanExpiresAt, now),
                addonThisMonth,
                (paying ? plan!.MonthlyPrice : 0) + addonThisMonth);
        })
            .OrderByDescending(x => x.RevenueThisMonth)
            .ThenBy(x => x.TenantName)
            .ToList();

        return new PlatformFinanceResponse(
            totals,
            planBreakdown,
            monthly,
            rows,
            tenants.Count,
            tenants.Count(t => t.IsActive && !t.IsPending),
            tenants.Count(t => t.IsPending),
            totals.Sum(t => t.PayingTenants));
    }

    private static DateTimeOffset PeriodStart(string period) =>
        new(int.Parse(period[..4]), int.Parse(period[5..]), 1, 0, 0, 0, TimeSpan.Zero);

    private static string Status(bool isActive, bool isPending, DateTimeOffset? expiresAt, DateTimeOffset now)
    {
        if (isPending) return "pending";
        if (!isActive) return "suspended";
        if (expiresAt is { } expires && expires < now) return "expired";
        return "active";
    }
}
