namespace LanguageLMS.Core.Modules.StudentApp.Services;

public record MembershipWindow(DateOnly Start, DateOnly? End);

public interface IStudentAccessService
{
    Task<bool> IsGroupMemberAsync(long studentId, long groupId, CancellationToken ct = default);

    Task<List<long>> AccessibleSubjectIdsAsync(long studentId, CancellationToken ct = default);

    Task<bool> HasSubjectAccessAsync(long studentId, long subjectId, CancellationToken ct = default);

    /// <summary>Şagirdin həmin fenn üzrə tarixli üzvülük pəncərələri (start_date olan). Boşdursa məhdudiyyət yox.</summary>
    Task<List<MembershipWindow>> MembershipWindowsAsync(long studentId, long subjectId, CancellationToken ct = default);
}
