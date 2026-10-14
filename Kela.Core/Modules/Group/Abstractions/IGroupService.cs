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

    Task<GroupResponse> GetAsync(long id, CancellationToken ct = default);

    Task<GroupResponse> CreateAsync(GroupUpsertRequest request, CancellationToken ct = default);

    Task<GroupResponse> UpdateAsync(long id, GroupUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task<IReadOnlyList<GroupMemberResponse>> ListStudentsAsync(long groupId, CancellationToken ct = default);

    Task<IReadOnlyList<GroupMemberResponse>> AssignStudentsAsync(long groupId, AssignStudentsRequest request, CancellationToken ct = default);

    Task RemoveStudentAsync(long groupId, long studentId, CancellationToken ct = default);

    Task<IReadOnlyList<SubjectResponse>> ListSubjectsAsync(long groupId, CancellationToken ct = default);

    Task<IReadOnlyList<SubjectResponse>> AssignSubjectsAsync(long groupId, AssignSubjectsRequest request, CancellationToken ct = default);

    Task RemoveSubjectAsync(long groupId, long subjectId, CancellationToken ct = default);

    Task<IReadOnlyList<QuizResponse>> ListQuizzesAsync(long groupId, CancellationToken ct = default);

    Task<IReadOnlyList<QuizResponse>> AssignQuizzesAsync(long groupId, AssignQuizzesRequest request, CancellationToken ct = default);

    Task RemoveQuizAsync(long groupId, long quizId, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<AssignmentLogResponse>> ListSubjectLogsAsync(long groupId, CancellationToken ct = default);
}
