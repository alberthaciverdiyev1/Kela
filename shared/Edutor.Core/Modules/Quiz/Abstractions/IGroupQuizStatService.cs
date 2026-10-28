using Edutor.Core.Modules.Quiz.Requests;
using Edutor.Core.Modules.Quiz.Responses;

namespace Edutor.Core.Modules.Quiz.Abstractions;

public interface IGroupQuizStatService
{
    Task<GroupQuizStatResponse> GetAsync(long groupId, GroupQuizStatRequest request, CancellationToken ct = default);

}
