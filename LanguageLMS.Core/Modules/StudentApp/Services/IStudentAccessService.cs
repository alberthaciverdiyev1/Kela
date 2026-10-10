namespace LanguageLMS.Core.Modules.StudentApp.Services;

public interface IStudentAccessService
{
    Task<bool> IsGroupMemberAsync(long studentId, long groupId, CancellationToken ct = default);

    Task<List<long>> AccessibleSubjectIdsAsync(long studentId, CancellationToken ct = default);

    Task<bool> HasSubjectAccessAsync(long studentId, long subjectId, CancellationToken ct = default);
}
