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
