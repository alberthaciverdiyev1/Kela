namespace Kela.Core.Modules.Payments.Responses;

public sealed record KassaCurrencyStats(
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

public sealed record KassaMonthStats(string Period, string Currency, decimal Amount);

public sealed record KassaPaymentRow(
    long Id, string StudentName, DateOnly PaidOn, decimal Amount, string Currency, string? Note);

public sealed record KassaOverviewResponse(
    IReadOnlyList<KassaCurrencyStats> Currencies,
    IReadOnlyList<KassaMonthStats> Monthly,
    IReadOnlyList<KassaPaymentRow> Recent,
    int ActivePlans,
    int OverduePlans,
    int DueSoonPlans,
    int StudentCount,
    int PayingStudents);
