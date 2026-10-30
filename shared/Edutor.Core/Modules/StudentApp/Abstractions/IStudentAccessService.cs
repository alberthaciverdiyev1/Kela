namespace Edutor.Core.Modules.StudentApp.Abstractions;

public record MembershipWindow(DateOnly Start, DateOnly? End);

public interface IStudentAccessService
{
    Task<bool> IsGroupMemberAsync(long studentId, long groupId, CancellationToken ct = default);

    /// <summary>Ödəniş məhdudiyyəti səbəbindən bağlanmış qruplar (boşdursa məhdudiyyət yox).</summary>
    Task<IReadOnlyList<long>> BlockedGroupIdsAsync(long studentId, CancellationToken ct = default);

    Task<List<long>> AccessibleSubjectIdsAsync(long studentId, CancellationToken ct = default);

    Task<bool> HasSubjectAccessAsync(long studentId, long subjectId, CancellationToken ct = default);

    /// <summary>Fənn açıqdırsa və dərs qrupun seçdiyi dərslər içindedirsə true.</summary>
    Task<bool> HasLessonAccessAsync(long studentId, long lessonId, CancellationToken ct = default);

    /// <summary>Şagirdin həmin fenn üzrə tarixli üzvülük pəncərələri (start_date olan). Boşdursa məhdudiyyət yox.</summary>
    Task<List<MembershipWindow>> MembershipWindowsAsync(long studentId, long subjectId, CancellationToken ct = default);
}
