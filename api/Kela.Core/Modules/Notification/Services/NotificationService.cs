using Kela.Core.Common.Requests;
using Kela.Core.Common.Sorting;
using Kela.Core.Common.Push;
using Kela.Core.Common.Pagination;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Notification.Requests;
using Kela.Core.Modules.Notification.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Notification.Services;

using Notification = Kela.Core.Modules.Notification.Entities.Notification;
using NotificationStudent = Kela.Core.Modules.Notification.Entities.NotificationStudent;
using NotificationToken = Kela.Core.Modules.Notification.Entities.NotificationToken;
using Kela.Core.Common.Queries;

public sealed class NotificationService(
    AppDbContext db,
    IFileStorage storage,
    ITenantFolder folders,
    IPushSender push) : INotificationService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/notifications";

    public async Task<NotificationResponse> CreateAsync(
        CreateNotificationRequest request, long? userId, CancellationToken ct = default)
    {
        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, await FolderAsync(ct), ct)
            : null;

        var notification = new Notification
        {
            Title = request.Title,
            Body = request.Body,
            Image = image,
            All = request.All,
            Data = request.Data,
            UserId = userId
        };

        if (!request.All)
        {
            foreach (var studentId in request.StudentIds.Distinct())
            {
                if (!await db.Students.AnyAsync(s => s.Id == studentId, ct))
                {
                    throw new ArgumentException($"Student not found: {studentId}");
                }

                notification.Recipients.Add(new NotificationStudent { StudentId = studentId });
            }
        }

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        // Push hazır deyil (native FCM/APNs provayderi yoxdur) — müvəqqəti söndürülüb.
        if (false)
        {
            await SendPushAsync(notification, request, ct);
        }

        return ToResponse(notification);
    }

    public async Task<PagedResponse<NotificationResponse>> ListAsync(
        NotificationListRequest request, CancellationToken ct = default)
    {
        var query = db.Notifications.AsNoTracking();

        query = query.ApplyTrashFilter(request.IsDeleted);

        query = query.SearchBy(request.Search, x => x.Title);

        return await PageAsync(query, request, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var notification = await db.Notifications
            .Include(x => x.Recipients)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Notification not found: {id}");

        foreach (var recipient in notification.Recipients)
        {
            recipient.DeletedAt = DateTimeOffset.UtcNow;
        }

        notification.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<NotificationResponse>> ListForStudentAsync(
        long studentId, NotificationListRequest request, CancellationToken ct = default)
    {
        var query = db.Notifications.AsNoTracking()
            .Where(n => n.All || db.NotificationStudents.Any(ns => ns.NotificationId == n.Id && ns.StudentId == studentId));

        return await PageAsync(query, request, ct);
    }

    public async Task SaveTokenAsync(long studentId, SaveNotificationTokenRequest request, CancellationToken ct = default)
    {
        var token = await db.NotificationTokens
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(t => t.Token == request.Token, ct);

        if (token is null)
        {
            db.NotificationTokens.Add(new NotificationToken
            {
                StudentId = studentId,
                Token = request.Token,
                DeviceType = request.DeviceType,
                IsActive = true,
                LastUsedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            token.StudentId = studentId;
            token.DeviceType = request.DeviceType;
            token.IsActive = true;
            token.LastUsedAt = DateTimeOffset.UtcNow;
            token.DeletedAt = null;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateTokenAsync(long studentId, string token, CancellationToken ct = default)
    {
        var existing = await db.NotificationTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.StudentId == studentId, ct);

        if (existing is not null)
        {
            existing.IsActive = false;
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task SendPushAsync(Notification notification, CreateNotificationRequest request, CancellationToken ct)
    {
        var tokensQuery = db.NotificationTokens.Where(t => t.IsActive);

        if (!request.All)
        {
            tokensQuery = tokensQuery.Where(t => request.StudentIds.Contains(t.StudentId));
        }

        var tokens = await tokensQuery.Select(t => t.Token).Distinct().ToListAsync(ct);
        if (tokens.Count == 0) return;

        await push.SendAsync(
            tokens,
            notification.Title,
            notification.Body,
            notification.Image is null ? null : storage.GetUrl(notification.Image),
            null,
            ct);
    }

    private async Task<PagedResponse<NotificationResponse>> PageAsync(
        IQueryable<Notification> query, ListRequestBase request, CancellationToken ct)
        => await query
            .ApplyListSort(request, x => x.Title)
            .ToPagedAsync(request, ToResponse, ct);

    private NotificationResponse ToResponse(Notification n) =>
        new(n.Id, n.Title, n.Body, storage.GetUrl(n.Image), n.All, n.CreatedAt, n.Data);
}
