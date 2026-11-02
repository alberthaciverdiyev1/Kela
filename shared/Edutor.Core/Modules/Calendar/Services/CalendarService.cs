using Edutor.Core.Common.Notes;
using Edutor.Core.Data;
using Edutor.Core.Modules.Calendar.Abstractions;
using Edutor.Core.Modules.Calendar.Entities;
using Edutor.Core.Modules.Calendar.Requests;
using Edutor.Core.Modules.Calendar.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Calendar.Services;

public sealed class CalendarService(AppDbContext db) : ICalendarService
{
    public async Task<IReadOnlyList<CalendarEventResponse>> GetRangeAsync(
        long userId, bool canSeeAllGroups, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var events = await db.TeacherEvents.AsNoTracking()
            .Where(x => x.UserId == userId && x.StartAt < to && x.EndAt >= from)
            .Select(x => new { x.Id, x.Title, x.Description, x.StartAt, x.EndAt, x.Color })
            .ToListAsync(ct);

        var fromDay = DateOnly.FromDateTime(from.LocalDateTime).AddDays(-1);
        var toDay = DateOnly.FromDateTime(to.LocalDateTime).AddDays(1);

        // DueDate DateTimeOffset-dur: gün sərhədləri üçün pəncərəni bir gün genişləndirib
        // yerli vaxtla günə bölürük (server saat qurşağı ilə saxlanılan offset fərqlənə bilər).
        var fromOffset = new DateTimeOffset(fromDay.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toOffset = new DateTimeOffset(toDay.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var homeworks = await db.HomeworkGroups.AsNoTracking()
            .Where(x => x.DueDate >= fromOffset && x.DueDate < toOffset
                        && x.Homework!.IsPublished
                        && (canSeeAllGroups || x.Group!.UserId == userId || x.Homework!.UserId == userId))
            .Select(x => new { x.DueDate, x.Homework!.Title, GroupName = x.Group!.Name })
            .ToListAsync(ct);

        var result = new List<CalendarEventResponse>();

        result.AddRange(events.Select(e => new CalendarEventResponse(
            e.Id,
            DateOnly.FromDateTime(e.StartAt.LocalDateTime),
            "event",
            e.Title,
            e.Description,
            null,
            Normalize(e.Color),
            false,
            e.StartAt,
            e.EndAt)));

        foreach (var h in homeworks)
        {
            var date = DateOnly.FromDateTime(h.DueDate.LocalDateTime);
            if (date < fromDay || date > toDay) continue;

            result.Add(new CalendarEventResponse(
                null, date, "homework", h.Title, h.GroupName, "/homework",
                Normalize(null), true, null, null));
        }

        return result.OrderBy(x => x.StartAt ?? x.Date.ToDateTime(TimeOnly.MinValue)).ToList();
    }

    public async Task<TeacherEventResponse> CreateEventAsync(
        long userId, TeacherEventUpsertRequest request, CancellationToken ct = default)
    {
        var entity = new TeacherEvent { UserId = userId };
        Apply(entity, request);

        db.TeacherEvents.Add(entity);
        await db.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task<TeacherEventResponse> UpdateEventAsync(
        long userId, long id, TeacherEventUpsertRequest request, CancellationToken ct = default)
    {
        var entity = await FindAsync(userId, id, ct);
        Apply(entity, request);
        await db.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task DeleteEventAsync(long userId, long id, CancellationToken ct = default)
    {
        var entity = await FindAsync(userId, id, ct);
        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task<TeacherEvent> FindAsync(long userId, long id, CancellationToken ct)
        => await db.TeacherEvents.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct)
            ?? throw new KeyNotFoundException($"Event not found: {id}");

    private static void Apply(TeacherEvent entity, TeacherEventUpsertRequest request)
    {
        entity.Title = request.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.StartAt = request.StartAt;
        entity.EndAt = request.EndAt;
        entity.Color = Normalize(request.Color);
    }

    private static string Normalize(string? color) => NoteColors.Normalize(color);

    private static TeacherEventResponse Map(TeacherEvent entity)
        => new(entity.Id, entity.Title, entity.Description, entity.StartAt, entity.EndAt, entity.Color);
}
