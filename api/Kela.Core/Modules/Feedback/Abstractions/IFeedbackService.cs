using Kela.Core.Common.Responses;
using Kela.Core.Modules.Feedback.Requests;
using Kela.Core.Modules.Feedback.Responses;

namespace Kela.Core.Modules.Feedback.Abstractions;

public interface IFeedbackService
{
    Task<FeedbackResponse> CreateAsync(CreateFeedbackRequest request, CancellationToken ct = default);

    Task<PagedResponse<FeedbackResponse>> ListAsync(FeedbackListRequest request, CancellationToken ct = default);

    Task<int> UnreadCountAsync(CancellationToken ct = default);

    Task MarkReadAsync(long id, bool isRead, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
