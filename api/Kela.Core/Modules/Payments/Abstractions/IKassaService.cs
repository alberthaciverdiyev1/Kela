using Kela.Core.Modules.Payments.Responses;

namespace Kela.Core.Modules.Payments.Abstractions;

public interface IKassaService
{
    /// <summary>Kassa icmalı: bu gün / bu ay / keçən ay / proqnoz, son ödənişlər və saylar.</summary>
    Task<KassaOverviewResponse> GetOverviewAsync(CancellationToken ct = default);

    /// <summary>Tarix aralığı üzrə bütün ödənişlər (CSV ixracı üçün).</summary>
    Task<IReadOnlyList<KassaPaymentRow>> ListPaymentsAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
}
