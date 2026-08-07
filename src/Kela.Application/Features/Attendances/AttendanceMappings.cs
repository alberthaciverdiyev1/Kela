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

    public static AttendanceMonthResponse ToMonthResponse(
        this Workspace workspace, int year, int month, IReadOnlyList<Attendance> records)
    {
        var students = workspace.Students
            .OrderBy(s => s.FirstName)
            .Select(s => new AttendanceStudentResponse(s.Id, $"{s.FirstName} {s.LastName}".Trim()))
            .ToList();

        var recordList = records
            .Select(r => new AttendanceRecordResponse(r.Id, r.StudentId, r.Date, r.Status, r.Note))
            .ToList();

        return new AttendanceMonthResponse(workspace.Id, year, month, students, recordList);
    }
}
