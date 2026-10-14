using Kela.Core.Common.Push;
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

public sealed class NotificationService(
    AppDbContext db,
    IFileStorage storage,
    IPushSender push) : INotificationService
{
    private const string ImageFolder = "notification";

    public async Task<NotificationResponse> CreateAsync(
        CreateNotificationRequest request, long? userId, CancellationToken ct = default)
    {
        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, ImageFolder, ct)
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

        await SendPushAsync(notification, request, ct);

        return ToResponse(notification);
    }

    public async Task<PagedResponse<NotificationResponse>> ListAsync(
        NotificationListRequest request, CancellationToken ct = default)
    {
        var (page, perPage) = Normalize(request);

        var query = db.Notifications.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search}%"));
        }

        return await PageAsync(query, page, perPage, ct);
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
        var (page, perPage) = Normalize(request);

        var query = db.Notifications.AsNoTracking()
            .Where(n => n.All || db.NotificationStudents.Any(ns => ns.NotificationId == n.Id && ns.StudentId == studentId));

        return await PageAsync(query, page, perPage, ct);
    }

    public async Task SaveTokenAsync(long studentId, SaveNotificationTokenRequest request, CancellationToken ct = default)
    {
        var token = await db.NotificationTokens
            .IgnoreQueryFilters()
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
        IQueryable<Notification> query, int page, int perPage, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<NotificationResponse>(data, page, lastPage, perPage, total);
    }

    private static (int page, int perPage) Normalize(NotificationListRequest request) =>
        (request.Page < 1 ? 1 : request.Page, request.PerPage is < 1 or > 200 ? 20 : request.PerPage);

    private NotificationResponse ToResponse(Notification n) =>
        new(n.Id, n.Title, n.Body, storage.GetUrl(n.Image), n.All, n.CreatedAt);
}
