using Kela.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.StudentApp.Services;

public sealed class StudentAccessService(AppDbContext db) : IStudentAccessService
{
    public Task<bool> IsGroupMemberAsync(long studentId, long groupId, CancellationToken ct = default)
        => db.StudentGroups.AnyAsync(sg => sg.GroupId == groupId && sg.StudentId == studentId, ct);

    public Task<List<long>> AccessibleSubjectIdsAsync(long studentId, CancellationToken ct = default)
        => db.GroupSubjects
            .Where(gs => db.StudentGroups.Any(sg => sg.GroupId == gs.GroupId && sg.StudentId == studentId))
            .Select(gs => gs.SubjectId)
            .Distinct()
            .ToListAsync(ct);

    public async Task<bool> HasSubjectAccessAsync(long studentId, long subjectId, CancellationToken ct = default)
        => (await AccessibleSubjectIdsAsync(studentId, ct)).Contains(subjectId);

    public async Task<bool> HasLessonAccessAsync(long studentId, long lessonId, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .Where(x => x.Id == lessonId)
            .Select(x => new { x.SubjectId })
            .FirstOrDefaultAsync(ct);

        if (lesson is null) return false;

        var links = await db.GroupSubjects.AsNoTracking()
            .Where(gs => gs.SubjectId == lesson.SubjectId
                && db.StudentGroups.Any(sg => sg.GroupId == gs.GroupId && sg.StudentId == studentId))
            .Select(gs => new { gs.GroupId, gs.LessonsConfigured })
            .ToListAsync(ct);

        if (links.Count == 0) return false;

        // Dərs seçimi etməyən köhnə qruplar üçün bütün dərslər açıqdır.
        if (links.Any(x => !x.LessonsConfigured)) return true;

        var groupIds = links.Select(x => x.GroupId).ToList();
        return await db.GroupLessons.AsNoTracking()
            .AnyAsync(gl => groupIds.Contains(gl.GroupId) && gl.LessonId == lessonId, ct);
    }

    public async Task<List<MembershipWindow>> MembershipWindowsAsync(long studentId, long subjectId, CancellationToken ct = default)
    {
        var rows = await db.StudentGroups
            .Where(sg => sg.StudentId == studentId
                && sg.StartDate != null
                && db.GroupSubjects.Any(gs => gs.GroupId == sg.GroupId && gs.SubjectId == subjectId))
            .Select(sg => new { sg.StartDate, sg.EndDate })
            .ToListAsync(ct);

        return rows.Select(r => new MembershipWindow(r.StartDate!.Value, r.EndDate)).ToList();
    }
}
