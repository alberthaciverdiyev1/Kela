using Edutor.Core.Common.Localization;
using Edutor.Core.Common.Pagination;
using Edutor.Core.Common.Queries;
using Edutor.Core.Common.Responses;
using Edutor.Core.Data;
using Edutor.Core.Modules.TeacherNote.Abstractions;
using Edutor.Core.Modules.TeacherNote.Requests;
using Edutor.Core.Modules.TeacherNote.Responses;
using Edutor.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.TeacherNote.Services;

using TeacherNote = Edutor.Core.Modules.TeacherNote.Entities.TeacherNote;
using TeacherNoteShare = Edutor.Core.Modules.TeacherNote.Entities.TeacherNoteShare;

public sealed class TeacherNoteService(AppDbContext db, IStringLocalizer<SharedResource> L) : ITeacherNoteService
{
    private const int MaxShareTargets = 200;
    private const int MaxContentOptions = 300;

    public async Task<PagedResponse<TeacherNoteResponse>> ListAsync(
        long userId, TeacherNoteListRequest request, CancellationToken ct = default)
    {
        var query = db.TeacherNotes.AsNoTracking()
            .Where(x => x.UserId == userId || x.Shares.Any(s => s.UserId == userId));

        query = query.SearchBy(request.Search, x => x.Title);

        if (request.IsDone is { } done) query = query.Where(x => x.IsDone == done);

        if (request.Scope == "mine") query = query.Where(x => x.UserId == userId);
        else if (request.Scope == "shared") query = query.Where(x => x.UserId != userId);

        query = query
            .OrderBy(x => x.Date == null)
            .ThenBy(x => x.Date)
            .ThenByDescending(x => x.CreatedAt);

        var (page, perPage) = Paging.Normalize(request);
        var total = await query.CountAsync(ct);
        var notes = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync(ct);

        var data = await MapAsync(notes, userId, ct);
        return new PagedResponse<TeacherNoteResponse>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }

    public async Task<TeacherNoteResponse> GetAsync(long userId, long id, CancellationToken ct = default)
    {
        var note = await FindAsync(userId, id, ct);
        return (await MapAsync([note], userId, ct))[0];
    }

    public async Task<TeacherNoteResponse> CreateAsync(
        long userId, TeacherNoteUpsertRequest request, CancellationToken ct = default)
    {
        // Paylaşım hədəfləri qeydə yazmadan ƏVVƏL yoxlanılır — xəta zamanı yarımçıq qeyd qalmasın.
        var targetIds = await ResolveShareTargetsAsync(userId, request, ct);

        var note = new TeacherNote { UserId = userId };
        await ApplyAsync(note, request, ct);

        db.TeacherNotes.Add(note);
        await db.SaveChangesAsync(ct);
        await ReplaceSharesAsync(note, targetIds, ct);

        return (await MapAsync([note], userId, ct))[0];
    }

    public async Task<TeacherNoteResponse> UpdateAsync(
        long userId, long id, TeacherNoteUpsertRequest request, CancellationToken ct = default)
    {
        var note = await FindAsync(userId, id, ct);
        var targetIds = await ResolveShareTargetsAsync(userId, request, ct);

        await ApplyAsync(note, request, ct);
        await db.SaveChangesAsync(ct);
        await ReplaceSharesAsync(note, targetIds, ct);

        return (await MapAsync([note], userId, ct))[0];
    }

    public async Task DeleteAsync(long userId, long id, CancellationToken ct = default)
    {
        var note = await FindAsync(userId, id, ct);
        note.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TeacherNoteUserResponse>> ListShareTargetsAsync(
        long userId, string? search = null, CancellationToken ct = default)
    {
        var query = db.Users.AsNoTracking()
            .Where(x => x.Id != userId && (x.Type == UserTypes.Teacher || x.Type == UserTypes.Admin || x.Type == UserTypes.SuperAdmin));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Email, pattern)
                || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                || (x.Surname != null && EF.Functions.ILike(x.Surname, pattern)));
        }

        var users = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Surname)
            .Take(MaxShareTargets)
            .Select(x => new { x.Id, x.Name, x.Surname, x.Type })
            .ToListAsync(ct);

        return users.Select(x => new TeacherNoteUserResponse(x.Id, DisplayName(x.Name, x.Surname), x.Type)).ToList();
    }

    public async Task<IReadOnlyList<TeacherNoteContentOption>> ListContentOptionsAsync(
        string? search = null, CancellationToken ct = default)
    {
        var query = from c in db.LessonContents.AsNoTracking()
                    join l in db.Lessons.AsNoTracking() on c.LessonId equals l.Id
                    select new { c.Id, ContentTitle = c.Title, LessonTitle = l.Title };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.ContentTitle, pattern) || EF.Functions.ILike(x.LessonTitle, pattern));
        }

        return await query
            .OrderBy(x => x.LessonTitle)
            .ThenBy(x => x.ContentTitle)
            .Take(MaxContentOptions)
            .Select(x => new TeacherNoteContentOption(x.Id, x.ContentTitle, x.LessonTitle, null))
            .ToListAsync(ct);
    }

    private async Task ApplyAsync(
        TeacherNote note, TeacherNoteUpsertRequest request, CancellationToken ct)
    {
        var visibility = request.Visibility == Entities.TeacherNoteVisibility.Shared
            ? Entities.TeacherNoteVisibility.Shared
            : Entities.TeacherNoteVisibility.Private;

        note.Title = request.Title.Trim();
        note.Body = string.IsNullOrWhiteSpace(request.Body) ? null : request.Body.Trim();
        note.Date = request.Date;
        note.IsDone = request.IsDone;
        note.ContentId = await ResolveContentAsync(request.ContentId, ct);
        note.PositionSeconds = note.ContentId is not null && request.PositionSeconds is >= 0
            ? request.PositionSeconds
            : null;
        note.Visibility = visibility;
    }

    private async Task<long?> ResolveContentAsync(long? contentId, CancellationToken ct)
    {
        if (contentId is not long id) return null;

        var exists = await db.LessonContents.AsNoTracking().AnyAsync(x => x.Id == id, ct);
        return exists ? id : throw new KeyNotFoundException($"Content not found: {id}");
    }

    private async Task ReplaceSharesAsync(
        TeacherNote note, IReadOnlyList<long> targetIds, CancellationToken ct)
    {
        var existing = await db.TeacherNoteShares.Where(x => x.TeacherNoteId == note.Id).ToListAsync(ct);

        foreach (var share in existing.Where(s => !targetIds.Contains(s.UserId)))
            db.TeacherNoteShares.Remove(share);

        foreach (var targetId in targetIds.Where(id => existing.All(s => s.UserId != id)))
            db.TeacherNoteShares.Add(new TeacherNoteShare { TeacherNoteId = note.Id, UserId = targetId });

        await db.SaveChangesAsync(ct);
    }

    private async Task<List<long>> ResolveShareTargetsAsync(
        long userId, TeacherNoteUpsertRequest request, CancellationToken ct)
    {
        if (request.Visibility != Entities.TeacherNoteVisibility.Shared) return [];

        var ids = request.SharedWithUserIds.Where(id => id != userId).Distinct().ToList();
        if (ids.Count == 0) throw new ArgumentException(L["Note_ShareTargetRequired"]);

        var found = await db.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && (x.Type == UserTypes.Teacher || x.Type == UserTypes.Admin || x.Type == UserTypes.SuperAdmin))
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (found.Count != ids.Count) throw new ArgumentException(L["Note_ShareTargetInvalid"]);
        return ids;
    }

    private async Task<TeacherNote> FindAsync(long userId, long id, CancellationToken ct)
        => await db.TeacherNotes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct)
            ?? throw new KeyNotFoundException($"Note not found: {id}");

    private async Task<IReadOnlyList<TeacherNoteResponse>> MapAsync(
        IReadOnlyList<TeacherNote> notes, long userId, CancellationToken ct)
    {
        if (notes.Count == 0) return [];

        var noteIds = notes.Select(x => x.Id).ToList();

        var authorIds = notes.Select(x => x.UserId).Distinct().ToList();
        var authors = (await db.Users.AsNoTracking()
                .Where(x => authorIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name, x.Surname })
                .ToListAsync(ct))
            .ToDictionary(x => x.Id, x => DisplayName(x.Name, x.Surname));

        var shareRows = await db.TeacherNoteShares.AsNoTracking()
            .Where(x => noteIds.Contains(x.TeacherNoteId))
            .Select(x => new { x.TeacherNoteId, x.UserId })
            .ToListAsync(ct);

        var shareUserIds = shareRows.Select(x => x.UserId).Distinct().ToList();
        var shareUsers = await db.Users.AsNoTracking()
            .Where(x => shareUserIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.Surname, x.Type })
            .ToDictionaryAsync(x => x.Id, ct);

        var contentIds = notes.Where(x => x.ContentId is not null).Select(x => x.ContentId!.Value).Distinct().ToList();
        var contents = await (from c in db.LessonContents.AsNoTracking()
                              join l in db.Lessons.AsNoTracking() on c.LessonId equals l.Id
                              where contentIds.Contains(c.Id)
                              select new { c.Id, c.Title, c.LessonId, LessonTitle = l.Title })
            .ToDictionaryAsync(x => x.Id, ct);

        return notes.Select(n =>
        {
            var content = n.ContentId is long cid ? contents.GetValueOrDefault(cid) : null;
            var sharedWith = shareRows
                .Where(s => s.TeacherNoteId == n.Id)
                .Select(s => shareUsers.GetValueOrDefault(s.UserId))
                .Where(u => u is not null)
                .Select(u => new TeacherNoteUserResponse(u!.Id, DisplayName(u.Name, u.Surname), u.Type))
                .ToList();

            return new TeacherNoteResponse(
                n.Id,
                n.UserId == userId,
                authors.GetValueOrDefault(n.UserId, string.Empty),
                n.Title,
                n.Body,
                n.Date,
                n.IsDone,
                n.ContentId,
                content?.Title,
                content?.LessonId,
                content?.LessonTitle,
                n.PositionSeconds,
                n.Visibility,
                sharedWith,
                n.CreatedAt,
                n.UpdatedAt);
        }).ToList();
    }

    private static string DisplayName(string? name, string? surname)
        => string.Join(' ', new[] { name, surname }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
