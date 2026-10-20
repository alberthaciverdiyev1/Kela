using Kela.Core.Common.Responses;
using Kela.Core.Modules.Payments.Requests;
using Kela.Core.Modules.Payments.Responses;

namespace Kela.Core.Modules.Payments.Abstractions;

public interface IStudentPaymentService
{
    Task<PagedResponse<StudentPaymentRow>> ListAsync(string? status, string? search, int page = 1, int perPage = 20, CancellationToken ct = default);

    /// <summary>Status üzrə ümumi saylar (statistika kartları üçün).</summary>
    Task<PaymentSummaryResponse> GetSummaryAsync(CancellationToken ct = default);

    /// <summary>Hazır plan şablonları — tələbəyə plan təyin edərkən seçilir.</summary>
    Task<IReadOnlyList<PaymentPlanTemplateResponse>> ListTemplatesAsync(CancellationToken ct = default);

    Task SaveTemplateAsync(SavePaymentPlanTemplateRequest request, CancellationToken ct = default);

    Task DeleteTemplateAsync(long id, CancellationToken ct = default);
    Task SavePlanAsync(SavePaymentPlanRequest request, CancellationToken ct = default);
    Task RecordPaymentAsync(RecordPaymentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<StudentPaymentHistory>> HistoryAsync(long studentId, CancellationToken ct = default);

    /// <summary>Şagird tətbiqi üçün cari plan və ödəniş tarixçəsi.</summary>
    Task<StudentPaymentOverviewResponse> GetStudentOverviewAsync(long studentId, CancellationToken ct = default);

    /// <summary>Ödəniş qeydini silir və planın vade tarixini yenidən hesablayır.</summary>
    Task DeletePaymentAsync(long paymentId, CancellationToken ct = default);
}
