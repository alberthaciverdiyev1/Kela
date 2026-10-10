using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Group.Requests;
using LanguageLMS.Core.Modules.Group.Responses;

namespace LanguageLMS.Core.Modules.Group.Services;

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
}
