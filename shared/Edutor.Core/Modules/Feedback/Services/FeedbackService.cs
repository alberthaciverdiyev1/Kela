using Edutor.Core.Common.Pagination;
using Edutor.Core.Common.Responses;
using Edutor.Core.Common.Storage;
using Edutor.Core.Data;
using Edutor.Core.Modules.Feedback.Abstractions;
using Edutor.Core.Modules.Feedback.Entities;
using Edutor.Core.Modules.Feedback.Requests;
using Edutor.Core.Modules.Feedback.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Feedback.Services;

using Feedback = Edutor.Core.Modules.Feedback.Entities.Feedback;
using FeedbackMessage = Edutor.Core.Modules.Feedback.Entities.FeedbackMessage;

public sealed class FeedbackService(AppDbContext db, IFileStorage storage) : IFeedbackService
{
    public async Task<FeedbackThreadResponse> CreateAsync(CreateFeedbackRequest request, CancellationToken ct = default)
    {
        var body = Normalize(request.Message);
        var image = request.Image is not null ? await storage.UploadAsync(request.Image, "feedback", ct) : null;

        var feedback = new Feedback
        {
            UserId = request.UserId,
            UserName = string.IsNullOrWhiteSpace(request.UserName) ? "—" : request.UserName.Trim(),
            UserEmail = request.UserEmail ?? string.Empty,
            Role = request.Role ?? string.Empty,
            LastMessageAt = DateTimeOffset.UtcNow
        };

        feedback.Messages.Add(new FeedbackMessage
        {
            // Mesaj söhbətin tenant-ına bağlanır (superadmin-in tokenında tenant olmaya bilər).
            TenantId = feedback.TenantId,
            SenderRole = FeedbackRoles.User,
            SenderName = feedback.UserName,
            Body = body,
            Image = image
        });

        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync(ct);

        return ToResponse(await LoadAsync(feedback.Id, ct));
    }

    public async Task<FeedbackThreadResponse> AddMessageAsync(long id, AddFeedbackMessageRequest request, CancellationToken ct = default)
    {
        var feedback = await db.Feedbacks.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Feedback not found: {id}");

        var body = Normalize(request.Message);
        var image = request.Image is not null ? await storage.UploadAsync(request.Image, "feedback", ct) : null;
        var isAdmin = request.SenderRole == FeedbackRoles.Admin;

        // Admin cavab yazanda qarşı tərəfin (şagirdin) mesajları oxunmuş sayılır.
        await MarkReadCoreAsync(feedback.Id, isAdmin ? FeedbackRoles.Admin : FeedbackRoles.User, ct);

        db.FeedbackMessages.Add(new FeedbackMessage
        {
            FeedbackId = feedback.Id,
            // Superadmin başqa tenant kontekstindedirsə belə, mesaj söhbətin tenant-ına yazılır.
            TenantId = feedback.TenantId,
            SenderRole = isAdmin ? FeedbackRoles.Admin : FeedbackRoles.User,
            SenderName = string.IsNullOrWhiteSpace(request.SenderName) ? "—" : request.SenderName.Trim(),
            Body = body,
            Image = image
        });

        feedback.LastMessageAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return ToResponse(await LoadAsync(feedback.Id, ct));
    }

    public async Task<PagedResponse<FeedbackThreadResponse>> ListAsync(FeedbackListRequest request, CancellationToken ct = default)
    {
        var query = db.Feedbacks.AsNoTracking();
        if (request.UserId is long userId) query = query.Where(x => x.UserId == userId);
        if (request.OnlyUnread) query = query.Where(x => x.Messages.Any(m => m.SenderRole == FeedbackRoles.User && !m.IsRead));

        return await query
            .Include(x => x.Messages.OrderBy(m => m.CreatedAt))
            .OrderByDescending(x => x.LastMessageAt)
            .ToPagedAsync(request, ToResponse, ct, maxPerPage: 100);
    }

    public Task<long?> OwnerIdAsync(long id, CancellationToken ct = default) =>
        db.Feedbacks.Where(x => x.Id == id).Select(x => x.UserId).FirstOrDefaultAsync(ct);

    public Task<int> UnreadCountAsync(CancellationToken ct = default) =>
        db.Feedbacks.CountAsync(x => x.Messages.Any(m => m.SenderRole == FeedbackRoles.User && !m.IsRead), ct);

    public async Task MarkReadAsync(long id, string readerRole, CancellationToken ct = default)
    {
        if (!await db.Feedbacks.AnyAsync(x => x.Id == id, ct))
            throw new KeyNotFoundException($"Feedback not found: {id}");

        await MarkReadCoreAsync(id, readerRole, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var feedback = await db.Feedbacks.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Feedback not found: {id}");

        feedback.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Oxuyan tərəf qarşı tərəfin mesajlarını oxunmuş edir.</summary>
    private async Task MarkReadCoreAsync(long feedbackId, string readerRole, CancellationToken ct)
    {
        var otherRole = readerRole == FeedbackRoles.Admin ? FeedbackRoles.User : FeedbackRoles.Admin;
        var unread = await db.FeedbackMessages
            .Where(m => m.FeedbackId == feedbackId && m.SenderRole == otherRole && !m.IsRead)
            .ToListAsync(ct);

        foreach (var m in unread) m.IsRead = true;
    }

    private Task<Feedback> LoadAsync(long id, CancellationToken ct) =>
        db.Feedbacks.AsNoTracking()
            .Include(x => x.Messages.OrderBy(m => m.CreatedAt))
            .FirstAsync(x => x.Id == id, ct);

    private static string Normalize(string? message)
    {
        var body = (message ?? string.Empty).Trim();
        if (body.Length == 0) throw new ArgumentException("Message is required");
        return body.Length > 4000 ? body[..4000] : body;
    }

    private FeedbackThreadResponse ToResponse(Feedback feedback)
    {
        var messages = feedback.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new FeedbackMessageResponse(
                m.Id,
                m.SenderRole,
                m.SenderName,
                m.Body,
                string.IsNullOrWhiteSpace(m.Image) ? null : storage.GetUrl(m.Image),
                m.IsRead,
                m.CreatedAt))
            .ToList();

        return new FeedbackThreadResponse(
            feedback.Id,
            feedback.UserId,
            feedback.UserName,
            feedback.UserEmail,
            feedback.Role,
            feedback.LastMessageAt,
            messages.Any(m => m.SenderRole == FeedbackRoles.User && !m.IsRead),
            messages);
    }
}
