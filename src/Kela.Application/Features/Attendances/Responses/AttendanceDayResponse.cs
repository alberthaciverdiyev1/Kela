namespace Kela.Application.Features.Attendances.Responses;

public sealed record AttendanceDayResponse(
    int WorkspaceId,
    DateOnly Date,
    IReadOnlyList<AttendanceEntryResponse> Entries);
