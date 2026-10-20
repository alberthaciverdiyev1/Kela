using Kela.Core.Common.Responses;
using Kela.Core.Modules.Payments.Requests;
using Kela.Core.Modules.Payments.Responses;

namespace Kela.Core.Modules.Payments.Abstractions;

public interface IStudentPaymentService
{
    Task<PagedResponse<StudentPaymentRow>> ListAsync(string? status, string? search, int page = 1, int perPage = 20, CancellationToken ct = default);

    /// <summary>Status üzrə ümumi saylar (statistika kartları üçün).</summary>
    Task<PaymentSummaryResponse> GetSummaryAsync(CancellationToken ct = default);

    /// <summary>Planı olmayan aktiv şagirdlər üçün standart plan yaradır; yaradılan sayı qaytarır.</summary>
    Task<int> CreateDefaultPlansAsync(CreateDefaultPlanRequest request, CancellationToken ct = default);
    Task SavePlanAsync(SavePaymentPlanRequest request, CancellationToken ct = default);
    Task RecordPaymentAsync(RecordPaymentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<StudentPaymentHistory>> HistoryAsync(long studentId, CancellationToken ct = default);
}
