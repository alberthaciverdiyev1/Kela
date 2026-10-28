using Edutor.Core.Data;
using Edutor.Core.Modules.Attendance.Abstractions;
using Edutor.Core.Modules.Attendance.Requests;
using Edutor.Core.Modules.Attendance.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Attendance.Services;

using AttendanceRecord = Edutor.Core.Modules.Attendance.Entities.AttendanceRecord;

public sealed class AttendanceService(AppDbContext db) : IAttendanceService
{
    public async Task<AttendanceDayResponse> GetDayAsync(long groupId, DateOnly date, CancellationToken ct = default)
    {
        await EnsureGroupAsync(groupId, ct);

        var members = await MembersAsync(groupId, ct);
        var saved = await db.AttendanceRecords.AsNoTracking()
            .Where(x => x.GroupId == groupId && x.Date == date)
            .ToDictionaryAsync(x => x.StudentId, x => x.IsPresent, ct);

        var students = members
            .Select(m =>
            {
                var recorded = saved.TryGetValue(m.StudentId, out var present);
                return new AttendanceStudentItem(m.StudentId, m.FullName, m.Email, recorded && present, recorded);
            })
            .ToList();

        return new AttendanceDayResponse(groupId, date, students.Count(s => s.IsPresent), students.Count, students);
    }

    public async Task<AttendanceDayResponse> SaveDayAsync(SaveAttendanceRequest request, CancellationToken ct = default)
    {
        await EnsureGroupAsync(request.GroupId, ct);

        // Yalnız qrupun üzvü olan şagirdlər qeyd olunur.
        var memberIds = (await MembersAsync(request.GroupId, ct)).Select(m => m.StudentId).ToHashSet();

        var existing = await db.AttendanceRecords
            .Where(x => x.GroupId == request.GroupId && x.Date == request.Date)
            .ToDictionaryAsync(x => x.StudentId, ct);

        foreach (var item in request.Students)
        {
            if (!memberIds.Contains(item.StudentId)) continue;

            if (existing.TryGetValue(item.StudentId, out var row))
            {
                row.IsPresent = item.IsPresent;
            }
            else
            {
                db.AttendanceRecords.Add(new AttendanceRecord
                {
                    GroupId = request.GroupId,
                    StudentId = item.StudentId,
                    Date = request.Date,
                    IsPresent = item.IsPresent
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return await GetDayAsync(request.GroupId, request.Date, ct);
    }

    public async Task<AttendanceMonthResponse> GetMonthAsync(long groupId, int year, int month, CancellationToken ct = default)
    {
        await EnsureGroupAsync(groupId, ct);

        var total = (await MembersAsync(groupId, ct)).Count;
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);

        var rows = await db.AttendanceRecords.AsNoTracking()
            .Where(x => x.GroupId == groupId && x.Date >= from && x.Date < to)
            .GroupBy(x => x.Date)
            .Select(g => new { Date = g.Key, Present = g.Count(x => x.IsPresent) })
            .ToListAsync(ct);

        var days = rows.Select(r => new AttendanceDayStat(r.Date, r.Present, total)).ToList();
        return new AttendanceMonthResponse(groupId, year, month, days);
    }

    public async Task<StudentAttendanceResponse> GetStudentSummaryAsync(long studentId, int take = 10, CancellationToken ct = default)
    {
        var rows = await db.AttendanceRecords.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.Date)
            .Select(x => new { x.Date, x.IsPresent, x.GroupId })
            .ToListAsync(ct);

        var groupIds = rows.Select(r => r.GroupId).Distinct().ToList();
        var names = await db.Groups.AsNoTracking()
            .Where(g => groupIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, g => g.Name, ct);

        var recent = rows.Take(take)
            .Select(r => new StudentAttendanceItem(r.Date, names.GetValueOrDefault(r.GroupId, string.Empty), r.IsPresent))
            .ToList();

        var present = rows.Count(r => r.IsPresent);
        var percent = rows.Count == 0 ? 0 : (int)Math.Round(present * 100.0 / rows.Count);

        return new StudentAttendanceResponse(rows.Count, present, rows.Count - present, percent, recent);
    }

    public async Task<StudentAttendanceMonthResponse> GetStudentMonthAsync(
        long studentId, long groupId, int year, int month, CancellationToken ct = default)
    {
        await EnsureGroupAsync(groupId, ct);

        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);

        var rows = await db.AttendanceRecords.AsNoTracking()
            .Where(x => x.StudentId == studentId && x.GroupId == groupId && x.Date >= from && x.Date < to)
            .OrderBy(x => x.Date)
            .Select(x => new { x.Date, x.IsPresent })
            .ToListAsync(ct);

        var days = rows.Select(r => new StudentAttendanceDayItem(r.Date, r.IsPresent)).ToList();
        return new StudentAttendanceMonthResponse(groupId, year, month, rows.Count(r => r.IsPresent), rows.Count, days);
    }

    private async Task EnsureGroupAsync(long groupId, CancellationToken ct)
    {
        if (!await db.Groups.AnyAsync(g => g.Id == groupId, ct))
            throw new KeyNotFoundException($"Group not found: {groupId}");
    }

    private Task<List<MemberRow>> MembersAsync(long groupId, CancellationToken ct) =>
        db.StudentGroups.AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .OrderBy(x => x.Id)
            .Select(x => new MemberRow(x.StudentId, x.Student!.FullName, x.Student!.Email))
            .ToListAsync(ct);

    private sealed record MemberRow(long StudentId, string FullName, string Email);
}
