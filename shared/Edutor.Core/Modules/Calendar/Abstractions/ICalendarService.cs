using Edutor.Core.Modules.Calendar.Requests;
using Edutor.Core.Modules.Calendar.Responses;

namespace Edutor.Core.Modules.Calendar.Abstractions;

public interface ICalendarService
{
    /// <summary>Verilmiş aralıqdakı hadisələr: müəllimin öz hadisələri + ev tapşırıqlarının son tarixləri.</summary>
    Task<IReadOnlyList<CalendarEventResponse>> GetRangeAsync(
        long userId, bool canSeeAllGroups, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    Task<TeacherEventResponse> CreateEventAsync(
        long userId, TeacherEventUpsertRequest request, CancellationToken ct = default);

    Task<TeacherEventResponse> UpdateEventAsync(
        long userId, long id, TeacherEventUpsertRequest request, CancellationToken ct = default);

    Task DeleteEventAsync(long userId, long id, CancellationToken ct = default);
}
