using Edutor.Core.Modules.Payments.Responses;

namespace Edutor.Core.Modules.Payments.Abstractions;

public interface ICashboxService
{
    /// <summary>Cashbox icmalı: bu gün / bu ay / keçən ay / proqnoz, son ödənişlər və saylar.</summary>
    Task<CashboxOverviewResponse> GetOverviewAsync(CancellationToken ct = default);

    /// <summary>Tarix aralığı üzrə bütün ödənişlər.</summary>
    Task<IReadOnlyList<CashboxPaymentRow>> ListPaymentsAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
}
