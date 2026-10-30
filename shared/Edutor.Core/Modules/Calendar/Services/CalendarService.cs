using Edutor.Core.Data;
using Edutor.Core.Modules.Calendar.Abstractions;
using Edutor.Core.Modules.Calendar.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Calendar.Services;

public sealed class CalendarService(AppDbContext db) : ICalendarService
{
    public async Task<IReadOnlyList<CalendarEventResponse>> GetMonthAsync(
        long userId, bool canSeeAllGroups, int year, int month, CancellationToken ct = default)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);

        var notes = await db.TeacherNotes.AsNoTracking()
            .Where(x => x.UserId == userId && x.Date != null && x.Date >= from && x.Date < to)
            .Select(x => new { Date = x.Date!.Value, x.Id, x.Title, x.IsDone })
            .ToListAsync(ct);

        // DueDate DateTimeOffset-dur: gün sərhədləri üçün pəncərəni bir gün genişləndirib
        // yerli vaxtla günə bölürük (server saat qurşağı ilə saxlanılan offset fərqlənə bilər).
        var fromOffset = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddDays(-1);
        var toOffset = new DateTimeOffset(to.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddDays(1);

        var homeworks = await db.HomeworkGroups.AsNoTracking()
            .Where(x => x.DueDate >= fromOffset && x.DueDate < toOffset
                        && x.Homework!.IsPublished
                        && (canSeeAllGroups || x.Group!.UserId == userId || x.Homework!.UserId == userId))
            .Select(x => new { x.DueDate, x.Homework!.Title, GroupName = x.Group!.Name })
            .ToListAsync(ct);

        var events = new List<CalendarEventResponse>();

        events.AddRange(notes.Select(n => new CalendarEventResponse(
            n.Date,
            "note",
            n.Title,
            null,
            "/teacher-note",
            n.IsDone)));

        foreach (var h in homeworks)
        {
            var date = DateOnly.FromDateTime(h.DueDate.LocalDateTime);
            if (date < from || date >= to) continue;

            events.Add(new CalendarEventResponse(date, "homework", h.Title, h.GroupName, "/homework", false));
        }

        return events.OrderBy(x => x.Date).ThenBy(x => x.Title).ToList();
    }
}
