using Edutor.Core.Data;
using Edutor.Core.Modules.Payments.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.StudentApp.Services;

public sealed class StudentAccessService(AppDbContext db, IPaymentAccessService paymentAccess) : IStudentAccessService
{
    public async Task<IReadOnlyList<long>> BlockedGroupIdsAsync(long studentId, CancellationToken ct = default)
    {
        var state = await paymentAccess.GetAsync(studentId, ct);
        if (state.BlockAll)
        {
            return await db.StudentGroups.AsNoTracking()
                .Where(sg => sg.StudentId == studentId)
                .Select(sg => sg.GroupId)
                .ToListAsync(ct);
        }

        return state.BlockedGroupIds;
    }

    public async Task<bool> IsGroupMemberAsync(long studentId, long groupId, CancellationToken ct = default)
        => await db.StudentGroups.AnyAsync(sg => sg.GroupId == groupId && sg.StudentId == studentId, ct)
           && !(await BlockedGroupIdsAsync(studentId, ct)).Contains(groupId);

    public async Task<List<long>> AccessibleSubjectIdsAsync(long studentId, CancellationToken ct = default)
    {
        var blocked = await BlockedGroupIdsAsync(studentId, ct);
        return await db.GroupSubjects
            .Where(gs => db.StudentGroups.Any(sg => sg.GroupId == gs.GroupId && sg.StudentId == studentId)
                         && !blocked.Contains(gs.GroupId))
            .Select(gs => gs.SubjectId)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<bool> HasSubjectAccessAsync(long studentId, long subjectId, CancellationToken ct = default)
        => (await AccessibleSubjectIdsAsync(studentId, ct)).Contains(subjectId);

    public async Task<bool> HasLessonAccessAsync(long studentId, long lessonId, CancellationToken ct = default)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .Where(x => x.Id == lessonId)
            .Select(x => new { x.SubjectId })
            .FirstOrDefaultAsync(ct);

        if (lesson is null) return false;

        var blocked = await BlockedGroupIdsAsync(studentId, ct);

        var links = await db.GroupSubjects.AsNoTracking()
            .Where(gs => gs.SubjectId == lesson.SubjectId
                && db.StudentGroups.Any(sg => sg.GroupId == gs.GroupId && sg.StudentId == studentId)
                && !blocked.Contains(gs.GroupId))
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
