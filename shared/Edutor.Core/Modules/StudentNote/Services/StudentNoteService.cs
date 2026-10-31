using Edutor.Core.Common.Exceptions;
using Edutor.Core.Common.Localization;
using Edutor.Core.Data;
using Edutor.Core.Modules.StudentApp.Abstractions;
using Edutor.Core.Modules.StudentNote.Abstractions;
using Edutor.Core.Modules.StudentNote.Entities;
using Edutor.Core.Modules.StudentNote.Requests;
using Edutor.Core.Modules.StudentNote.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Edutor.Core.Modules.StudentNote.Services;

using StudentNote = Edutor.Core.Modules.StudentNote.Entities.StudentNote;

public sealed class StudentNoteService(
    AppDbContext db,
    IStudentAccessService access,
    IStringLocalizer<SharedResource> L) : IStudentNoteService
{
    public async Task<IReadOnlyList<StudentNoteResponse>> ListAsync(
        long studentId, StudentNoteListRequest request, CancellationToken ct = default)
    {
        var myGroupIds = await db.StudentGroups.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Select(x => x.GroupId)
            .ToListAsync(ct);

        var query = db.StudentNotes.AsNoTracking().Where(n =>
            n.StudentId == studentId
            || (n.Visibility == StudentNoteVisibility.Group
                && n.GroupId != null && myGroupIds.Contains(n.GroupId.Value)));

        if (request.ContentId is long contentId) query = query.Where(x => x.LessonContentId == contentId);
        if (request.LessonId is long lessonId) query = query.Where(x => x.LessonId == lessonId);
        if (request.OnlyFree is true) query = query.Where(x => x.LessonContentId == null);
        if (request.Scope == "mine") query = query.Where(x => x.StudentId == studentId);
        else if (request.Scope == "shared") query = query.Where(x => x.StudentId != studentId);

        var notes = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        return await MapAsync(notes, studentId, ct);
    }

    public async Task<StudentNoteResponse> CreateAsync(
        long studentId, StudentNoteUpsertRequest request, CancellationToken ct = default)
    {
        var lessonId = await ResolveLessonAsync(request.ContentId, ct);

        if (lessonId is long lid && !await access.HasLessonAccessAsync(studentId, lid, ct))
        {
            throw new ForbiddenException("You do not have access to this lesson");
        }

        var visibility = NormalizeVisibility(request.Visibility);
        var groupId = await ResolveGroupAsync(studentId, lessonId, visibility, request.GroupId, ct);

        var note = new StudentNote
        {
            StudentId = studentId,
            LessonId = lessonId,
            LessonContentId = request.ContentId,
            PositionSeconds = request.PositionSeconds is >= 0 ? request.PositionSeconds : null,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            Body = request.Body.Trim(),
            Visibility = visibility,
            GroupId = groupId
        };

        db.StudentNotes.Add(note);
        await db.SaveChangesAsync(ct);

        return (await MapAsync([note], studentId, ct))[0];
    }

    public async Task<StudentNoteResponse> UpdateAsync(
        long studentId, long id, StudentNoteUpsertRequest request, CancellationToken ct = default)
    {
        var note = await db.StudentNotes.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId, ct)
            ?? throw new KeyNotFoundException($"Note not found: {id}");

        var lessonId = await ResolveLessonAsync(request.ContentId, ct);

        if (lessonId is long lid && !await access.HasLessonAccessAsync(studentId, lid, ct))
        {
            throw new ForbiddenException("You do not have access to this lesson");
        }

        var visibility = NormalizeVisibility(request.Visibility);
        var groupId = await ResolveGroupAsync(studentId, lessonId, visibility, request.GroupId, ct);

        note.LessonId = lessonId;
        note.LessonContentId = request.ContentId;
        note.PositionSeconds = request.PositionSeconds is >= 0 ? request.PositionSeconds : null;
        note.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
        note.Body = request.Body.Trim();
        note.Visibility = visibility;
        note.GroupId = groupId;

        await db.SaveChangesAsync(ct);
        return (await MapAsync([note], studentId, ct))[0];
    }

    public async Task DeleteAsync(long studentId, long id, CancellationToken ct = default)
    {
        var note = await db.StudentNotes.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId, ct)
            ?? throw new KeyNotFoundException($"Note not found: {id}");

        note.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private static string NormalizeVisibility(string? visibility)
        => visibility == StudentNoteVisibility.Group ? StudentNoteVisibility.Group : StudentNoteVisibility.Private;

    private async Task<long?> ResolveLessonAsync(long? contentId, CancellationToken ct)
    {
        if (contentId is not long id) return null;

        var lessonId = await db.LessonContents.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => (long?)x.LessonId)
            .FirstOrDefaultAsync(ct);

        return lessonId ?? throw new KeyNotFoundException($"Content not found: {id}");
    }

    private async Task<long?> ResolveGroupAsync(
        long studentId, long? lessonId, string visibility, long? requestedGroupId, CancellationToken ct)
    {
        if (visibility != StudentNoteVisibility.Group) return null;

        var myGroupIds = await db.StudentGroups.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Select(x => x.GroupId)
            .ToListAsync(ct);

        if (myGroupIds.Count == 0) throw new ArgumentException(L["StudentNote_GroupRequired"]);

        if (requestedGroupId is long groupId)
        {
            if (!myGroupIds.Contains(groupId)) throw new ForbiddenException(L["StudentNote_NotGroupMember"]);
            return groupId;
        }

        // Materialın fənni hansı qruplaşmada açıqdırsa, ilk növbədə həmin qrup seçilir.
        if (lessonId is long lid)
        {
            var subjectId = await db.Lessons.AsNoTracking()
                .Where(x => x.Id == lid)
                .Select(x => (long?)x.SubjectId)
                .FirstOrDefaultAsync(ct);

            if (subjectId is long sid)
            {
                var groupForSubject = await db.GroupSubjects.AsNoTracking()
                    .Where(x => x.SubjectId == sid && myGroupIds.Contains(x.GroupId))
                    .Select(x => (long?)x.GroupId)
                    .FirstOrDefaultAsync(ct);

                if (groupForSubject is long gid) return gid;
            }
        }

        return myGroupIds[0];
    }

    private async Task<IReadOnlyList<StudentNoteResponse>> MapAsync(
        IReadOnlyList<StudentNote> notes, long studentId, CancellationToken ct)
    {
        if (notes.Count == 0) return [];

        var studentIds = notes.Select(x => x.StudentId).Distinct().ToList();
        var authors = await db.Students.AsNoTracking()
            .Where(x => studentIds.Contains(x.Id))
            .Select(x => new { x.Id, x.FullName })
            .ToDictionaryAsync(x => x.Id, x => x.FullName, ct);

        var lessonIds = notes.Where(x => x.LessonId is not null).Select(x => x.LessonId!.Value).Distinct().ToList();
        var lessons = await db.Lessons.AsNoTracking()
            .Where(x => lessonIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Title })
            .ToDictionaryAsync(x => x.Id, x => x.Title, ct);

        var contentIds = notes.Where(x => x.LessonContentId is not null).Select(x => x.LessonContentId!.Value).Distinct().ToList();
        var contents = await db.LessonContents.AsNoTracking()
            .Where(x => contentIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Title })
            .ToDictionaryAsync(x => x.Id, x => x.Title, ct);

        var groupIds = notes.Where(x => x.GroupId is not null).Select(x => x.GroupId!.Value).Distinct().ToList();
        var groups = await db.Groups.AsNoTracking()
            .Where(x => groupIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return notes.Select(n => new StudentNoteResponse(
            n.Id,
            n.StudentId == studentId,
            authors.GetValueOrDefault(n.StudentId, string.Empty),
            n.LessonId,
            n.LessonId is long lid ? lessons.GetValueOrDefault(lid) : null,
            n.LessonContentId,
            n.LessonContentId is long cid ? contents.GetValueOrDefault(cid) : null,
            n.PositionSeconds,
            n.Title,
            n.Body,
            n.Visibility,
            n.GroupId,
            n.GroupId is long gid ? groups.GetValueOrDefault(gid) : null,
            n.CreatedAt,
            n.UpdatedAt)).ToList();
    }
}
