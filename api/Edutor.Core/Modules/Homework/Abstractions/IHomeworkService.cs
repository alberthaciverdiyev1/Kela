using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Homework.Requests;
using Edutor.Core.Modules.Homework.Responses;

namespace Edutor.Core.Modules.Homework.Abstractions;

public interface IHomeworkService
{
    Task<PagedResponse<HomeworkResponse>> ListAsync(HomeworkListRequest request, long? ownerId = null, CancellationToken ct = default);
    Task<HomeworkResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default);
    Task<HomeworkResponse> CreateAsync(HomeworkUpsertRequest request, long? ownerId = null, CancellationToken ct = default);
    Task<HomeworkResponse> UpdateAsync(long id, HomeworkUpsertRequest request, long? ownerId = null, CancellationToken ct = default);
    Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default);
    Task<HomeworkResponse> PublishAsync(long id, long? ownerId = null, CancellationToken ct = default);

    Task<PagedResponse<HomeworkAssignmentResponse>> ListAssignmentsAsync(
        long homeworkId, HomeworkAssignmentListRequest request, long? ownerId = null, CancellationToken ct = default);

    Task<HomeworkAssignmentResponse> GradeAsync(
        long homeworkId, long studentId, GradeHomeworkRequest request, long? ownerId = null,
        long? graderId = null, CancellationToken ct = default);
}
