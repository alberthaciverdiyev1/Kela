using Kela.Core.Modules.Payments.Requests;
using Kela.Core.Modules.Payments.Responses;

namespace Kela.Core.Modules.Payments.Abstractions;

public interface IStudentPaymentService
{
    Task<IReadOnlyList<StudentPaymentRow>> ListAsync(string? status, string? search, CancellationToken ct = default);
    Task SavePlanAsync(SavePaymentPlanRequest request, CancellationToken ct = default);
    Task RecordPaymentAsync(RecordPaymentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<StudentPaymentHistory>> HistoryAsync(long studentId, CancellationToken ct = default);
}
