using Kela.Domain.Common;

namespace Kela.Application.Features.Attendances.Responses;

public sealed record AttendanceEntryResponse(
    int StudentId,
    string StudentName,
    AttendanceStatus Status,
    string? Note,
    int? AttendanceId);
