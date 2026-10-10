using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;

namespace LanguageLMS.Core.Modules.Quiz.Services;

public interface IGroupQuizStatService
{
    Task<GroupQuizStatResponse> GetAsync(long groupId, GroupQuizStatRequest request, CancellationToken ct = default);
}
