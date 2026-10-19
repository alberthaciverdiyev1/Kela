using Kela.Core.Modules.Quiz.Requests;
using Kela.Core.Modules.Quiz.Responses;

namespace Kela.Core.Modules.Quiz.Abstractions;

public interface IGroupQuizStatService
{
    Task<GroupQuizStatResponse> GetAsync(long groupId, GroupQuizStatRequest request, CancellationToken ct = default);

}
