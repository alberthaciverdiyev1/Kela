using Kela.Domain.Common;

namespace Kela.Application.Features.Attendances.Responses;

public sealed record AttendanceRecordResponse(
    int Id,
    int StudentId,
    DateOnly Date,
    AttendanceStatus Status,
    string? Note);
