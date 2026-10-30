using Edutor.Core.Common.Pagination;
using Edutor.Core.Common.Queries;
using Edutor.Core.Common.Responses;
using Edutor.Core.Data;
using Edutor.Core.Modules.TeacherNote.Abstractions;
using Edutor.Core.Modules.TeacherNote.Requests;
using Edutor.Core.Modules.TeacherNote.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.TeacherNote.Services;

using TeacherNote = Edutor.Core.Modules.TeacherNote.Entities.TeacherNote;

public sealed class TeacherNoteService(AppDbContext db) : ITeacherNoteService
{
    public async Task<PagedResponse<TeacherNoteResponse>> ListAsync(
        long userId, TeacherNoteListRequest request, CancellationToken ct = default)
    {
        var query = db.TeacherNotes.AsNoTracking().Where(x => x.UserId == userId);

        query = query.SearchBy(request.Search, x => x.Title);

        if (request.IsDone is { } done) query = query.Where(x => x.IsDone == done);

        return await query
            .OrderBy(x => x.Date == null)
            .ThenBy(x => x.Date)
            .ThenByDescending(x => x.CreatedAt)
            .ToPagedAsync(request, ToResponse, ct);
    }

    public async Task<TeacherNoteResponse> GetAsync(long userId, long id, CancellationToken ct = default)
        => ToResponse(await FindAsync(userId, id, ct));

    public async Task<TeacherNoteResponse> CreateAsync(
        long userId, TeacherNoteUpsertRequest request, CancellationToken ct = default)
    {
        var note = new TeacherNote { UserId = userId };
        Apply(note, request);

        db.TeacherNotes.Add(note);
        await db.SaveChangesAsync(ct);

        return ToResponse(note);
    }

    public async Task<TeacherNoteResponse> UpdateAsync(
        long userId, long id, TeacherNoteUpsertRequest request, CancellationToken ct = default)
    {
        var note = await FindAsync(userId, id, ct);
        Apply(note, request);

        await db.SaveChangesAsync(ct);
        return ToResponse(note);
    }

    public async Task DeleteAsync(long userId, long id, CancellationToken ct = default)
    {
        var note = await FindAsync(userId, id, ct);
        note.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task<TeacherNote> FindAsync(long userId, long id, CancellationToken ct)
        => await db.TeacherNotes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct)
            ?? throw new KeyNotFoundException($"Note not found: {id}");

    private static void Apply(TeacherNote note, TeacherNoteUpsertRequest request)
    {
        note.Title = request.Title.Trim();
        note.Body = string.IsNullOrWhiteSpace(request.Body) ? null : request.Body.Trim();
        note.Date = request.Date;
        note.IsDone = request.IsDone;
    }

    private static TeacherNoteResponse ToResponse(TeacherNote n) =>
        new(n.Id, n.Title, n.Body, n.Date, n.IsDone, n.CreatedAt);
}
