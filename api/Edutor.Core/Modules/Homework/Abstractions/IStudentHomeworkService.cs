using Edutor.Core.Modules.Homework.Requests;
using Edutor.Core.Modules.Homework.Responses;

namespace Edutor.Core.Modules.Homework.Abstractions;

public interface IStudentHomeworkService
{
    Task<IReadOnlyList<StudentHomeworkSummaryResponse>> ListAsync(long studentId, CancellationToken ct = default);
    Task<StudentHomeworkDetailResponse> GetAsync(long studentId, long homeworkId, CancellationToken ct = default);
    Task<StudentHomeworkDetailResponse> SubmitAsync(long studentId, long homeworkId, SubmitHomeworkRequest request, CancellationToken ct = default);
}
