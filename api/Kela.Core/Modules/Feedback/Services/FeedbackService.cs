using Kela.Core.Common.Responses;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Feedback.Abstractions;
using Kela.Core.Modules.Feedback.Entities;
using Kela.Core.Modules.Feedback.Requests;
using Kela.Core.Modules.Feedback.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Feedback.Services;

using Feedback = Kela.Core.Modules.Feedback.Entities.Feedback;

public sealed class FeedbackService(AppDbContext db, IFileStorage storage) : IFeedbackService
{
    public async Task<FeedbackResponse> CreateAsync(CreateFeedbackRequest request, CancellationToken ct = default)
    {
        var message = (request.Message ?? string.Empty).Trim();
        if (message.Length == 0) throw new ArgumentException("Message is required");
        if (message.Length > 4000) message = message[..4000];

        var image = request.Image is not null
            ? await storage.UploadAsync(request.Image, "feedback", ct)
            : null;

        var feedback = new Feedback
        {
            UserId = request.UserId,
            UserName = string.IsNullOrWhiteSpace(request.UserName) ? "—" : request.UserName.Trim(),
            UserEmail = request.UserEmail ?? string.Empty,
            Role = request.Role ?? string.Empty,
            Message = message,
            Image = image,
            Status = FeedbackStatus.New
        };

        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync(ct);

        return ToResponse(feedback);
    }

    public async Task<PagedResponse<FeedbackResponse>> ListAsync(FeedbackListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage < 1 ? 20 : Math.Min(request.PerPage, 100);

        var query = db.Feedbacks.AsNoTracking();
        if (request.OnlyNew) query = query.Where(x => x.Status == FeedbackStatus.New);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);
        return new PagedResponse<FeedbackResponse>(data, page, lastPage, perPage, total);
    }

    public Task<int> UnreadCountAsync(CancellationToken ct = default) =>
        db.Feedbacks.CountAsync(x => x.Status == FeedbackStatus.New, ct);

    public async Task MarkReadAsync(long id, bool isRead, CancellationToken ct = default)
    {
        var feedback = await db.Feedbacks.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Feedback not found: {id}");

        feedback.Status = isRead ? FeedbackStatus.Read : FeedbackStatus.New;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var feedback = await db.Feedbacks.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Feedback not found: {id}");

        feedback.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private FeedbackResponse ToResponse(Feedback feedback) =>
        new(
            feedback.Id,
            feedback.UserId,
            feedback.UserName,
            feedback.UserEmail,
            feedback.Role,
            feedback.Message,
            string.IsNullOrWhiteSpace(feedback.Image) ? null : storage.GetUrl(feedback.Image),
            feedback.Status,
            feedback.CreatedAt);
}
