namespace Edutor.Core.Modules.Subscription.Responses;

public sealed record PlatformCurrencyTotals(
    string Currency,
    int PayingTenants,
    decimal Mrr,
    decimal Arr,
    decimal RevenueThisMonth,
    decimal RevenueLastMonth,
    decimal AddonThisMonth,
    decimal RevenueAllTime);

public sealed record PlatformPlanBreakdown(
    long PlanId,
    string PlanName,
    string Currency,
    decimal MonthlyPrice,
    int Tenants,
    decimal Mrr);

public sealed record PlatformMonthPoint(
    string Period,
    string Currency,
    decimal Subscriptions,
    decimal Addons)
{
    public decimal Total => Subscriptions + Addons;
}

public sealed record PlatformTenantRow(
    long TenantId,
    string TenantName,
    string? PlanName,
    decimal MonthlyPrice,
    string Currency,
    DateTimeOffset? PlanStartedAt,
    DateTimeOffset? PlanExpiresAt,
    string Status,
    decimal AddonThisMonth,
    decimal RevenueThisMonth);

public sealed record PlatformFinanceResponse(
    IReadOnlyList<PlatformCurrencyTotals> Currencies,
    IReadOnlyList<PlatformPlanBreakdown> Plans,
    IReadOnlyList<PlatformMonthPoint> Monthly,
    IReadOnlyList<PlatformTenantRow> Tenants,
    int TotalTenants,
    int ActiveTenants,
    int PendingTenants,
    int PayingTenants);
