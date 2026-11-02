using Edutor.Core.Common.Entities;
using Edutor.Core.Data;
using Edutor.Core.Modules.Sidebar.Abstractions;
using Edutor.Core.Modules.Sidebar.Entities;
using Edutor.Core.Modules.Sidebar.Requests;
using Edutor.Core.Modules.Sidebar.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Sidebar.Services;

using AddonRequestStatuses = Edutor.Core.Modules.Subscription.Entities.AddonRequestStatuses;
using FeedbackRoles = Edutor.Core.Modules.Feedback.Entities.FeedbackRoles;
using GroupJoinStatus = Edutor.Core.Modules.Group.Entities.GroupJoinStatus;

public sealed class SidebarService(AppDbContext db) : ISidebarService
{
    // Heç vaxt baxılmamış bölmələr üçün "yeni" pəncərəsi — ilk yükləmədə hər şey işarələnməsin.
    private static readonly TimeSpan UnseenWindow = TimeSpan.FromDays(7);

    public async Task<SidebarSummaryResponse> GetSummaryAsync(long userId, CancellationToken ct = default)
    {
        var seen = userId > 0
            ? await db.Users.AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.SidebarSeen)
                .FirstOrDefaultAsync(ct) ?? []
            : [];

        var response = new SidebarSummaryResponse();

        async Task CountWithNewAsync<T>(string key, IQueryable<T> query) where T : BaseEntity
        {
            response.Counts[key] = await query.CountAsync(ct);

            var since = seen.TryGetValue(key, out var value) ? value : DateTimeOffset.UtcNow - UnseenWindow;
            if (await query.AnyAsync(x => x.CreatedAt > since, ct)) response.NewKeys.Add(key);
        }

        await CountWithNewAsync("subject", db.Subjects.AsNoTracking());
        await CountWithNewAsync("lesson", db.Lessons.AsNoTracking());
        await CountWithNewAsync("lesson-content", db.LessonContents.AsNoTracking());
        await CountWithNewAsync("question", db.Questions.AsNoTracking());
        await CountWithNewAsync("quiz", db.Quizzes.AsNoTracking());
        await CountWithNewAsync("homework", db.Homeworks.AsNoTracking());
        await CountWithNewAsync("attendance", db.AttendanceRecords.AsNoTracking());
        await CountWithNewAsync("calendar", db.TeacherEvents.AsNoTracking());
        await CountWithNewAsync("teacher-note", db.TeacherNotes.AsNoTracking());
        await CountWithNewAsync("group", db.Groups.AsNoTracking());
        await CountWithNewAsync("students", db.Students.AsNoTracking());
        await CountWithNewAsync("banner", db.Banners.AsNoTracking());
        await CountWithNewAsync("user", db.Users.AsNoTracking());
        await CountWithNewAsync("node", db.WorkerNodes.AsNoTracking());

        var plans = await db.SubscriptionPlans.AsNoTracking().CountAsync(ct);
        response.Counts["plan"] = plans;
        response.Counts["plan-catalog"] = plans;

        var pendingJoins = await db.GroupJoinRequests.AsNoTracking()
            .CountAsync(x => x.Status == GroupJoinStatus.Pending, ct);
        response.Counts["group-request"] = pendingJoins;
        if (pendingJoins > 0) response.NewKeys.Add("group-request");

        var pendingAddons = await db.AddonRequests.AsNoTracking()
            .CountAsync(x => x.Status == AddonRequestStatuses.Pending, ct);
        response.Counts["addon-request"] = pendingAddons;
        if (pendingAddons > 0) response.NewKeys.Add("addon-request");

        var pendingTenants = await db.Tenants.AsNoTracking().CountAsync(x => x.IsPending, ct);
        response.Counts["tenant"] = pendingTenants;
        if (pendingTenants > 0) response.NewKeys.Add("tenant");

        var unreadFeedback = await db.Feedbacks.AsNoTracking()
            .CountAsync(x => x.Messages.Any(m => m.SenderRole == FeedbackRoles.User && !m.IsRead), ct);
        response.Counts["feedback"] = await db.Feedbacks.AsNoTracking().CountAsync(ct);
        if (unreadFeedback > 0) response.NewKeys.Add("feedback");

        // Ödənişi yaxınlaşan planlar — kassada diqqət işarəsi.
        var dueUntil = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);
        var duePlans = await db.StudentPaymentPlans.AsNoTracking()
            .CountAsync(x => x.NextDueOn <= dueUntil, ct);
        var payments = await db.StudentPayments.AsNoTracking().CountAsync(ct);
        response.Counts["payment"] = payments;
        response.Counts["cashbox"] = payments;
        if (duePlans > 0)
        {
            response.NewKeys.Add("payment");
            response.NewKeys.Add("cashbox");
        }

        var labels = await db.SidebarLabels.AsNoTracking()
            .Where(x => x.IsActive && x.Label != "")
            .ToListAsync(ct);
        foreach (var label in labels) response.Labels[label.Key] = label.Label;

        return response;
    }

    public async Task MarkSeenAsync(long userId, string key, CancellationToken ct = default)
    {
        if (userId <= 0 || SidebarKeys.Find(key) is null) return;

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null) return;

        var now = DateTimeOffset.UtcNow;
        if (user.SidebarSeen.TryGetValue(key, out var last) && now - last < TimeSpan.FromSeconds(30)) return;

        user.SidebarSeen[key] = now;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SidebarLabelResponse>> ListLabelsAsync(CancellationToken ct = default)
        => await db.SidebarLabels.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new SidebarLabelResponse { Key = x.Key, Label = x.Label, IsActive = x.IsActive })
            .ToListAsync(ct);

    public async Task<SidebarLabelResponse> SetLabelAsync(string key, SidebarLabelRequest request, CancellationToken ct = default)
    {
        var item = SidebarKeys.Find(key) ?? throw new KeyNotFoundException($"Sidebar key not found: {key}");

        var label = request.Label?.Trim() ?? string.Empty;
        var entity = await db.SidebarLabels.FirstOrDefaultAsync(x => x.Key == item.Key, ct);

        if (label.Length == 0 || !request.IsActive)
        {
            if (entity is not null) db.SidebarLabels.Remove(entity);
            await db.SaveChangesAsync(ct);
            return new SidebarLabelResponse { Key = item.Key, Label = string.Empty, IsActive = false };
        }

        if (label.Length > 40) throw new ArgumentException("Label is too long");

        if (entity is null)
        {
            entity = new SidebarLabel { Key = item.Key, Label = label, IsActive = true };
            db.SidebarLabels.Add(entity);
        }
        else
        {
            entity.Label = label;
            entity.IsActive = true;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return new SidebarLabelResponse { Key = entity.Key, Label = entity.Label, IsActive = entity.IsActive };
    }
}
