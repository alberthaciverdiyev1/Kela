using Kela.Core.Common.Pagination;
using Kela.Core.Common.Responses;
using Kela.Core.Data;
using Kela.Core.Modules.Payments.Entities;
using Kela.Core.Modules.Payments.Requests;
using Kela.Core.Modules.Payments.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Payments.Services;

public sealed class StudentPaymentService(AppDbContext db) : Abstractions.IStudentPaymentService
{
    public async Task<PagedResponse<StudentPaymentRow>> ListAsync(
        string? status, string? search, int page = 1, int perPage = 20, CancellationToken ct = default)
    {
        (page, perPage) = Paging.Normalize(page, perPage);

        var rows = await BuildRowsAsync(search, ct);

        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            rows = rows.Where(x => x.Status == status).ToList();
        }

        var total = rows.Count;
        var data = rows.Skip((page - 1) * perPage).Take(perPage).ToList();

        return new PagedResponse<StudentPaymentRow>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }

    public async Task<PaymentSummaryResponse> GetSummaryAsync(CancellationToken ct = default)
    {
        var rows = await BuildRowsAsync(null, ct);

        return new PaymentSummaryResponse(
            rows.Count(x => x.Status == "upcoming"),
            rows.Count(x => x.Status == "overdue"),
            rows.Count(x => x.Status == "unpaid"),
            rows.Count(x => x.Status == "current"));
    }

    public async Task<IReadOnlyList<PaymentPlanTemplateResponse>> ListTemplatesAsync(CancellationToken ct = default)
        => await db.Set<PaymentPlanTemplate>().AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new PaymentPlanTemplateResponse(x.Id, x.Name, x.Amount, x.Currency, x.Frequency, x.IntervalDays, x.Note))
            .ToListAsync(ct);

    public async Task SaveTemplateAsync(SavePaymentPlanTemplateRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (name.Length == 0) throw new ArgumentException("Şablon adı tələb olunur.");

        ValidateFrequency(request.Frequency, request.IntervalDays);
        if (request.Amount <= 0) throw new ArgumentException("Ödəniş məbləği sıfırdan böyük olmalıdır.");

        PaymentPlanTemplate template;
        if (request.Id is long id)
        {
            template = await db.Set<PaymentPlanTemplate>().FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new KeyNotFoundException("Şablon tapılmadı.");
        }
        else
        {
            template = new PaymentPlanTemplate();
            db.Add(template);
        }

        template.Name = name;
        template.Amount = request.Amount;
        template.Currency = PaymentCurrencies.Normalize(request.Currency);
        template.Frequency = request.Frequency;
        template.IntervalDays = request.Frequency == "custom" ? request.IntervalDays : null;
        template.Note = request.Note?.Trim();
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteTemplateAsync(long id, CancellationToken ct = default)
    {
        var template = await db.Set<PaymentPlanTemplate>().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Şablon tapılmadı.");

        template.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task<List<StudentPaymentRow>> BuildRowsAsync(string? search, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var students = await db.Students.AsNoTracking()
            .Where(s => string.IsNullOrWhiteSpace(search) || EF.Functions.ILike(s.FullName, $"%{search!.Trim()}%") || EF.Functions.ILike(s.Email, $"%{search.Trim()}%"))
            .Select(s => new { s.Id, s.FullName, s.Email, s.IsActive, Plan = db.Set<StudentPaymentPlan>().FirstOrDefault(p => p.StudentId == s.Id) })
            .ToListAsync(ct);

        return students.Select(x =>
        {
            var p = x.Plan;
            var state = p is null ? "unpaid" : p.NextDueOn < today ? "overdue" : p.NextDueOn <= today.AddDays(7) ? "upcoming" : "current";
            return new StudentPaymentRow(x.Id, x.FullName, x.Email, x.IsActive, p?.JoinedOn, p?.NextDueOn, p?.LastPaidOn, p?.Amount, p?.Currency, p?.Frequency, p?.IntervalDays, state, p?.Note);
        })
        .OrderBy(x => x.NextDueOn ?? DateOnly.MinValue)
        .ThenBy(x => x.FullName)
        .ToList();
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
        plan.Currency = PaymentCurrencies.Normalize(request.Currency);
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
    {
        var student = await db.Students.AsNoTracking()
            .Where(x => x.Id == studentId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(ct);

        if (student is null) return Array.Empty<StudentPaymentHistory>();

        var payments = await db.Set<StudentPayment>().AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.PaidOn)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        return payments
            .Select(p => new StudentPaymentHistory(p.Id, p.StudentId, student, p.PaidOn, p.Amount, p.Currency, p.Note))
            .ToList();
    }

    private static DateOnly NextDue(DateOnly due, DateOnly paidOn, string frequency, int? interval)
    {
        // Növbəti ödəniş hələ gələcəkdədirsə (ilk ödəniş/prepayment) onu dəyişmirik.
        if (due > paidOn) return due;

        var next = due;
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
