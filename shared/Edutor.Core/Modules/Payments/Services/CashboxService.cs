using Edutor.Core.Data;
using Edutor.Core.Modules.Payments.Abstractions;
using Edutor.Core.Modules.Payments.Entities;
using Edutor.Core.Modules.Payments.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Payments.Services;

public sealed class CashboxService(AppDbContext db) : ICashboxService
{
    public async Task<CashboxOverviewResponse> GetOverviewAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var historyStart = monthStart.AddMonths(-11);

        var payments = await db.Set<StudentPayment>().AsNoTracking()
            .Where(p => p.PaidOn >= historyStart && p.PaidOn < nextMonthStart)
            .Select(p => new { p.Id, p.StudentId, p.PaidOn, p.Amount, p.Currency, p.Note })
            .ToListAsync(ct);

        var plans = await db.Set<StudentPaymentPlan>().AsNoTracking()
            .Select(p => new { p.StudentId, p.Amount, p.Currency, p.Frequency, p.IntervalDays, p.NextDueOn })
            .ToListAsync(ct);

        var activeStudents = await db.Students.AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => s.Id)
            .ToListAsync(ct);
        var activeSet = activeStudents.ToHashSet();

        var currencyCodes = payments.Select(p => p.Currency)
            .Concat(plans.Where(p => activeSet.Contains(p.StudentId)).Select(p => p.Currency))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        if (currencyCodes.Count == 0) currencyCodes.Add("AZN");

        var stats = currencyCodes.Select(currency =>
        {
            var paid = payments.Where(p => p.Currency == currency).ToList();
            var activePlans = plans.Where(p => p.Currency == currency && activeSet.Contains(p.StudentId)).ToList();

            return new CashboxCurrencyStats(
                currency,
                Today: paid.Where(p => p.PaidOn == today).Sum(p => p.Amount),
                ThisMonth: paid.Where(p => p.PaidOn >= monthStart && p.PaidOn < nextMonthStart).Sum(p => p.Amount),
                ExpectedThisMonth: activePlans.Where(p => p.NextDueOn < nextMonthStart).Sum(p => p.Amount),
                LastMonth: paid.Where(p => p.PaidOn >= lastMonthStart && p.PaidOn < monthStart).Sum(p => p.Amount),
                ProjectedNextMonth: activePlans.Sum(p => MonthlyEquivalent(p.Frequency, p.IntervalDays, p.Amount)),
                OverdueAmount: activePlans.Where(p => p.NextDueOn < today).Sum(p => p.Amount),
                ActivePlans: activePlans.Count);
        }).ToList();

        var monthly = (from offset in Enumerable.Range(0, 12)
                       let periodStart = historyStart.AddMonths(offset)
                       let periodEnd = periodStart.AddMonths(1)
                       from currency in currencyCodes
                       select new CashboxMonthStats(
                           periodStart.ToString("yyyy-MM"),
                           currency,
                           payments.Where(p => p.Currency == currency && p.PaidOn >= periodStart && p.PaidOn < periodEnd).Sum(p => p.Amount)))
            .ToList();

        var studentNames = await db.Students.AsNoTracking()
            .Select(s => new { s.Id, s.FullName })
            .ToDictionaryAsync(x => x.Id, x => x.FullName, ct);

        var recent = payments
            .OrderByDescending(p => p.PaidOn).ThenByDescending(p => p.Id)
            .Take(15)
            .Select(p => new CashboxPaymentRow(
                p.Id,
                studentNames.GetValueOrDefault(p.StudentId, $"#{p.StudentId}"),
                p.PaidOn, p.Amount, p.Currency, p.Note))
            .ToList();

        return new CashboxOverviewResponse(
            stats,
            monthly,
            recent,
            ActivePlans: plans.Count(p => activeSet.Contains(p.StudentId)),
            OverduePlans: plans.Count(p => activeSet.Contains(p.StudentId) && p.NextDueOn < today),
            DueSoonPlans: plans.Count(p => activeSet.Contains(p.StudentId) && p.NextDueOn >= today && p.NextDueOn <= today.AddDays(7)),
            StudentCount: activeStudents.Count,
            PayingStudents: plans.Where(p => activeSet.Contains(p.StudentId)).Select(p => p.StudentId).Distinct().Count());
    }

    public async Task<IReadOnlyList<CashboxPaymentRow>> ListPaymentsAsync(
        DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var query = db.Set<StudentPayment>().AsNoTracking();

        if (from is DateOnly start) query = query.Where(p => p.PaidOn >= start);
        if (to is DateOnly end) query = query.Where(p => p.PaidOn <= end);

        var rows = await query
            .OrderByDescending(p => p.PaidOn).ThenByDescending(p => p.Id)
            .Take(1000)
            .ToListAsync(ct);
        var studentIds = rows.Select(r => r.StudentId).Distinct().ToList();

        var names = await db.Students.AsNoTracking()
            .Where(s => studentIds.Contains(s.Id))
            .Select(s => new { s.Id, s.FullName })
            .ToDictionaryAsync(x => x.Id, x => x.FullName, ct);

        return rows
            .Select(r => new CashboxPaymentRow(
                r.Id, names.GetValueOrDefault(r.StudentId, $"#{r.StudentId}"),
                r.PaidOn, r.Amount, r.Currency, r.Note))
            .ToList();
    }

    /// <summary>Planı aylıq gəlirə çevirir (proqnoz üçün).</summary>
    private static decimal MonthlyEquivalent(string frequency, int? intervalDays, decimal amount)
        => frequency switch
        {
            "custom" when intervalDays is > 0 => amount * 30m / intervalDays.Value,
            _ => amount
        };
}
