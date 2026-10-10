using LanguageLMS.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.StudentApp.Services;

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
}
