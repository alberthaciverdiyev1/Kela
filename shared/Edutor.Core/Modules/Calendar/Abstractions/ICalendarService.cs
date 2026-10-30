using Edutor.Core.Modules.Calendar.Responses;

namespace Edutor.Core.Modules.Calendar.Abstractions;

public interface ICalendarService
{
    /// <summary>Ay üzrə hadisələr: yayımlanmış ev tapşırıqlarının son tarixləri + tarixli şəxsi qeydlər.</summary>
    Task<IReadOnlyList<CalendarEventResponse>> GetMonthAsync(
        long userId, bool canSeeAllGroups, int year, int month, CancellationToken ct = default);
}
