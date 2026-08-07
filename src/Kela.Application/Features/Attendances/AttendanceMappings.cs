using Kela.Application.Features.Attendances.Responses;
using Kela.Domain.Common;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Attendances;

public static class AttendanceMappings
{
    public static AttendanceDayResponse ToDayResponse(this Workspace workspace, DateOnly date, IReadOnlyList<Attendance> records)
    {
        var byStudent = records.ToDictionary(r => r.StudentId);

        var entries = workspace.Students
            .OrderBy(s => s.FirstName)
            .Select(s =>
            {
                byStudent.TryGetValue(s.Id, out var record);
                return new AttendanceEntryResponse(
                    s.Id,
                    $"{s.FirstName} {s.LastName}".Trim(),
                    record?.Status ?? AttendanceStatus.Unknown,
                    record?.Note,
                    record?.Id);
            })
            .ToList();

        return new AttendanceDayResponse(workspace.Id, date, entries);
    }
}
