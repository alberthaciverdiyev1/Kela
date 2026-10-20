namespace Kela.App.Shared.Dtos;

public record StudentPaymentHistoryDto(
    long Id,
    long StudentId,
    string StudentName,
    DateOnly PaidOn,
    decimal Amount,
    string Currency,
    string? Note);

public record StudentPaymentOverviewDto(
    decimal? Amount,
    string? Currency,
    string? Frequency,
    int? IntervalDays,
    DateOnly? JoinedOn,
    DateOnly? NextDueOn,
    DateOnly? LastPaidOn,
    string? Note,
    string Status,
    List<StudentPaymentHistoryDto> History);
