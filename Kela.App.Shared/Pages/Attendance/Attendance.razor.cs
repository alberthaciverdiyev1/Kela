using System.Globalization;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Services.Localization;

namespace Kela.App.Shared.Pages.Attendance;

public partial class Attendance : RefreshablePage
{
    private bool _loading = true;
    private List<GroupDto> _groups = [];
    private long _groupId;
    private DateTime _month = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private Dictionary<int, bool> _marks = [];
    private int _presentCount;
    private int _totalCount;

    protected override void OnInitialized() => OnRefresh(FetchAsync);

    protected override async Task OnInitializedAsync()
    {
        await FetchAsync();
        _loading = false;
    }

    private async Task FetchAsync()
    {
        try
        {
            _groups = await Api.StudentGroupsAsync();
            if (_groupId == 0 && _groups.Count > 0) _groupId = _groups[0].Id;
        }
        catch
        {
            /* şəbəkə xətası — boş qalır */
        }

        await LoadMonthAsync();
    }

    private async Task LoadMonthAsync()
    {
        _marks = [];
        _presentCount = 0;
        _totalCount = 0;

        if (_groupId == 0) return;

        try
        {
            var data = await Api.MyAttendanceMonthAsync(_groupId, _month.Year, _month.Month);
            _marks = data.Days.ToDictionary(d => d.Date.Day, d => d.IsPresent);
            _presentCount = data.PresentCount;
            _totalCount = data.TotalCount;
        }
        catch
        {
            /* sükutla */
        }
    }

    private Task SelectGroupAsync(long id)
    {
        _groupId = id;
        return LoadMonthAsync();
    }

    private Task PrevMonthAsync()
    {
        _month = _month.AddMonths(-1);
        return LoadMonthAsync();
    }

    private Task NextMonthAsync()
    {
        _month = _month.AddMonths(1);
        return LoadMonthAsync();
    }

    /// <summary>Təqvim günü adları və ay adı seçilmiş dildə göstərilir; ICU dəstəkləmirsə fallback.</summary>
    private CultureInfo Culture
    {
        get
        {
            try { return CultureInfo.GetCultureInfo(L.Current); }
            catch { return CultureInfo.InvariantCulture; }
        }
    }

    private string MonthLabel => _month.ToString("MMMM yyyy", Culture);

    private string[] Weekdays
    {
        get
        {
            var names = Culture.DateTimeFormat.AbbreviatedDayNames;
            // Bazar ertəsi birinci gün olsun (DateTimeFormat-da bazar 0-dır).
            return Enumerable.Range(1, 7).Select(i => names[i % 7]).ToArray();
        }
    }

    private int DaysInMonth => DateTime.DaysInMonth(_month.Year, _month.Month);

    private int Leading => ((int)new DateTime(_month.Year, _month.Month, 1).DayOfWeek + 6) % 7;

    private string DayClass(int day)
    {
        var today = DateTime.Today;
        var isToday = _month.Year == today.Year && _month.Month == today.Month && day == today.Day;
        var ring = isToday ? " ring-2 ring-brand-467 ring-offset-1" : "";

        if (!_marks.TryGetValue(day, out var present))
            return "text-sage-355" + ring;

        return (present ? "bg-brand-103 text-brand-835" : "bg-[#fdeceb] text-danger-539") + ring;
    }
}
