namespace Edutor.Core.Modules.Payments.Responses;

public sealed record CashboxCurrencyStats(
    string Currency,
    decimal Today,
    decimal ThisMonth,
    /// <summary>Bu ay yığılması gözlənilən məbləğ: vaxtı keçmiş + bu ay ödənilməli planlar.</summary>
    decimal ExpectedThisMonth,
    decimal LastMonth,
    /// <summary>Aktiv planların aylıq ekvivalentinə əsaslanan təxmini gəlir.</summary>
    decimal ProjectedNextMonth,
    decimal OverdueAmount,
    int ActivePlans);

public sealed record CashboxMonthStats(string Period, string Currency, decimal Amount);

public sealed record CashboxPaymentRow(
    long Id, string StudentName, DateOnly PaidOn, decimal Amount, string Currency, string? Note);

public sealed record CashboxOverviewResponse(
    IReadOnlyList<CashboxCurrencyStats> Currencies,
    IReadOnlyList<CashboxMonthStats> Monthly,
    IReadOnlyList<CashboxPaymentRow> Recent,
    int ActivePlans,
    int OverduePlans,
    int DueSoonPlans,
    int StudentCount,
    int PayingStudents);
