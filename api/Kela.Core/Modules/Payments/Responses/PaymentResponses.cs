namespace Kela.Core.Modules.Payments.Responses;

public sealed record StudentPaymentRow(long StudentId, string FullName, string Email, bool IsActive, DateOnly? JoinedOn, DateOnly? NextDueOn, DateOnly? LastPaidOn, decimal? Amount, string? Currency, string? Frequency, int? IntervalDays, string Status, string? Note);
public sealed record StudentPaymentHistory(long Id, long StudentId, string StudentName, DateOnly PaidOn, decimal Amount, string Currency, string? Note);
