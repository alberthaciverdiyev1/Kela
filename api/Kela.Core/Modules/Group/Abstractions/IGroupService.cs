using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Group.Requests;
using Kela.Core.Modules.Group.Responses;
using Kela.Core.Modules.Quiz.Responses;
using Kela.Core.Modules.Subject.Responses;

namespace Kela.Core.Modules.Group.Abstractions;

public interface IGroupService
{
    Task<PagedResponse<GroupResponse>> ListAsync(GroupListRequest request, CancellationToken ct = default);

    /// <summary><paramref name="ownerId"/> verilərsə yalnız öz/admin qrupuna giriş icazəsi verilir.</summary>
    Task<GroupResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<GroupResponse> CreateAsync(GroupUpsertRequest request, CancellationToken ct = default);

    Task<GroupResponse> UpdateAsync(long id, GroupUpsertRequest request, long? ownerId = null, CancellationToken ct = default);

    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<GroupMemberResponse>> ListStudentsAsync(long groupId, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<GroupMemberResponse>> AssignStudentsAsync(long groupId, AssignStudentsRequest request, long? ownerId = null, CancellationToken ct = default);

    Task RemoveStudentAsync(long groupId, long studentId, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<SubjectResponse>> ListSubjectsAsync(long groupId, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<SubjectResponse>> AssignSubjectsAsync(long groupId, AssignSubjectsRequest request, long? ownerId = null, CancellationToken ct = default);

    Task RemoveSubjectAsync(long groupId, long subjectId, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<QuizResponse>> ListQuizzesAsync(long groupId, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<QuizResponse>> AssignQuizzesAsync(long groupId, AssignQuizzesRequest request, long? ownerId = null, CancellationToken ct = default);

    Task RemoveQuizAsync(long groupId, long quizId, long? ownerId = null, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default);

    Task<IReadOnlyList<AssignmentLogResponse>> ListSubjectLogsAsync(long groupId, long? ownerId = null, CancellationToken ct = default);

    /// <summary>Linklə qoşulma müraciətləri (teacher yalnız öz qrupu).</summary>
    Task<IReadOnlyList<GroupJoinRequestResponse>> ListJoinRequestsAsync(long groupId, long? ownerId = null, CancellationToken ct = default);

    /// <summary>Müraciəti təsdiqlə (üzvlük yaranır) və ya rədd et.</summary>
    Task DecideJoinRequestAsync(long groupId, long requestId, bool approve, long? ownerId = null, CancellationToken ct = default);
}
