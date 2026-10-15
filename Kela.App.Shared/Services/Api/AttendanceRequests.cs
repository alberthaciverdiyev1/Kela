using Kela.App.Shared.Dtos.Attendance;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Yoxlama ---
    public Task<MyAttendanceDto> MyAttendanceAsync() => _api.GetAsync<MyAttendanceDto>("/api/attendance/my");

    public Task<MyAttendanceMonthDto> MyAttendanceMonthAsync(long groupId, int year, int month)
        => _api.GetAsync<MyAttendanceMonthDto>($"/api/attendance/my/month?group_id={groupId}&year={year}&month={month}");
}
