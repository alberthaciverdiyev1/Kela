using Kela.Core.Data;
using Kela.Core.Modules.SystemNotification.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.SystemNotification.Services;

using SystemNotification = Kela.Core.Modules.SystemNotification.Entities.SystemNotification;

public sealed class SystemNotificationService(AppDbContext db) : Abstractions.ISystemNotificationService
{
    public async Task<IReadOnlyList<SystemNotificationResponse>> ListAsync(long recipientId, CancellationToken ct = default)
        => await db.SystemNotifications.AsNoTracking()
            .Where(x => x.RecipientId == recipientId)
            .OrderByDescending(x => x.Id)
            .Select(x => new SystemNotificationResponse(x.Id, x.Title, x.Body, x.ReadAt != null, x.CreatedAt))
            .ToListAsync(ct);

    public async Task<SystemNotificationResponse> CreateAsync(long recipientId, string title, string? body, CancellationToken ct = default)
    {
        if (!await db.Users.AnyAsync(u => u.Id == recipientId, ct))
        {
            throw new ArgumentException($"User not found: {recipientId}");
        }

        var notification = new SystemNotification { RecipientId = recipientId, Title = title, Body = body };
        db.SystemNotifications.Add(notification);
        await db.SaveChangesAsync(ct);

        return new SystemNotificationResponse(notification.Id, notification.Title, notification.Body, false, notification.CreatedAt);
    }

    public async Task MarkReadAsync(long recipientId, long id, CancellationToken ct = default)
    {
        var notification = await db.SystemNotifications
            .FirstOrDefaultAsync(x => x.Id == id && x.RecipientId == recipientId, ct)
            ?? throw new KeyNotFoundException($"Notification not found: {id}");

        notification.ReadAt ??= DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(long recipientId, CancellationToken ct = default)
    {
        var unread = await db.SystemNotifications
            .Where(x => x.RecipientId == recipientId && x.ReadAt == null)
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        foreach (var n in unread) n.ReadAt = now;

        await db.SaveChangesAsync(ct);
    }
}
