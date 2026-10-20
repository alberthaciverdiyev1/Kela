using Kela.Core.Data;
using Kela.Core.Modules.Payments.Entities;
using Kela.Core.Modules.Payments.Requests;
using Kela.Core.Modules.Payments.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Payments.Services;

public sealed class StudentPaymentService(AppDbContext db) : Abstractions.IStudentPaymentService
{
    public async Task<IReadOnlyList<StudentPaymentRow>> ListAsync(string? status, string? search, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var students = await db.Students.AsNoTracking()
            .Where(s => string.IsNullOrWhiteSpace(search) || EF.Functions.ILike(s.FullName, $"%{search!.Trim()}%") || EF.Functions.ILike(s.Email, $"%{search.Trim()}%"))
            .Select(s => new { s.Id, s.FullName, s.Email, s.IsActive, Plan = db.Set<StudentPaymentPlan>().FirstOrDefault(p => p.StudentId == s.Id) })
            .ToListAsync(ct);

        var rows = students.Select(x =>
        {
            var p = x.Plan;
            var state = p is null ? "unpaid" : p.NextDueOn < today ? "overdue" : p.NextDueOn <= today.AddDays(7) ? "upcoming" : "current";
            return new StudentPaymentRow(x.Id, x.FullName, x.Email, x.IsActive, p?.JoinedOn, p?.NextDueOn, p?.LastPaidOn, p?.Amount, p?.Currency, p?.Frequency, p?.IntervalDays, state, p?.Note);
        }).ToList();
        if (!string.IsNullOrWhiteSpace(status) && status != "all") rows = rows.Where(x => x.Status == status).ToList();
        return rows.OrderBy(x => x.NextDueOn ?? DateOnly.MinValue).ThenBy(x => x.FullName).ToList();
    }

    public async Task SavePlanAsync(SavePaymentPlanRequest request, CancellationToken ct = default)
    {
        ValidateFrequency(request.Frequency, request.IntervalDays);
        if (request.Amount <= 0) throw new ArgumentException("Ödəniş məbləği sıfırdan böyük olmalıdır.");
        if (!await db.Students.AnyAsync(x => x.Id == request.StudentId, ct)) throw new KeyNotFoundException("Şagird tapılmadı.");
        var plan = await db.Set<StudentPaymentPlan>().IgnoreQueryFilters(new[] { "SoftDelete" }).FirstOrDefaultAsync(x => x.StudentId == request.StudentId, ct);
        if (plan is null) { plan = new StudentPaymentPlan { StudentId = request.StudentId }; db.Add(plan); }
        plan.DeletedAt = null;
        plan.JoinedOn = request.JoinedOn;
        plan.NextDueOn = request.NextDueOn;
        plan.Amount = request.Amount;
        plan.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "AZN" : request.Currency.Trim().ToUpperInvariant();
        plan.Frequency = request.Frequency;
        plan.IntervalDays = request.Frequency == "custom" ? request.IntervalDays : null;
        plan.Note = request.Note?.Trim();
        await db.SaveChangesAsync(ct);
    }

    public async Task RecordPaymentAsync(RecordPaymentRequest request, CancellationToken ct = default)
    {
        var plan = await db.Set<StudentPaymentPlan>().FirstOrDefaultAsync(x => x.StudentId == request.StudentId, ct)
            ?? throw new InvalidOperationException("Ödəniş qeyd etməzdən əvvəl şagird üçün ödəniş planı yaradın.");
        var paidOn = request.PaidOn;
        db.Add(new StudentPayment { StudentId = request.StudentId, PaidOn = paidOn, Amount = request.Amount ?? plan.Amount, Currency = plan.Currency, Note = request.Note?.Trim() });
        plan.LastPaidOn = paidOn;
        plan.NextDueOn = NextDue(plan.NextDueOn, paidOn, plan.Frequency, plan.IntervalDays);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StudentPaymentHistory>> HistoryAsync(long studentId, CancellationToken ct = default)
        => await db.Set<StudentPayment>().AsNoTracking().Where(x => x.StudentId == studentId)
            .Join(db.Students, p => p.StudentId, s => s.Id, (p, s) => new StudentPaymentHistory(p.Id, p.StudentId, s.FullName, p.PaidOn, p.Amount, p.Currency, p.Note))
            .OrderByDescending(x => x.PaidOn).ToListAsync(ct);

    private static DateOnly NextDue(DateOnly due, DateOnly paidOn, string frequency, int? interval)
    {
        var next = due > paidOn ? due : paidOn;
        if (frequency == "custom")
        {
            do { next = next.AddDays(interval!.Value); } while (next <= paidOn);
            return next;
        }
        DateOnly candidate;
        do { candidate = next.AddMonths(1); next = candidate; } while (candidate <= paidOn);
        return candidate;
    }

    private static void ValidateFrequency(string frequency, int? days)
    {
        if (frequency == "monthly") return;
        if (frequency == "custom" && days is > 0 and <= 365) return;
        throw new ArgumentException("Ödəniş periodu aylıq və ya 1–365 gün arası xüsusi interval olmalıdır.");
    }
}
