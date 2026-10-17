using Kela.Core.Common.Responses;
using Kela.Core.Modules.Feedback.Requests;
using Kela.Core.Modules.Feedback.Responses;

namespace Kela.Core.Modules.Feedback.Abstractions;

public interface IFeedbackService
{
    Task<FeedbackThreadResponse> CreateAsync(CreateFeedbackRequest request, CancellationToken ct = default);

    Task<FeedbackThreadResponse> AddMessageAsync(long id, AddFeedbackMessageRequest request, CancellationToken ct = default);

    Task<PagedResponse<FeedbackThreadResponse>> ListAsync(FeedbackListRequest request, CancellationToken ct = default);

    Task<long?> OwnerIdAsync(long id, CancellationToken ct = default);

    Task<int> UnreadCountAsync(CancellationToken ct = default);

    Task MarkReadAsync(long id, string readerRole, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
