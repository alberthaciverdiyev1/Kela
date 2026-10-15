using Kela.App.Shared.Dtos.Attendance;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Yoxlama ---
    public Task<MyAttendanceDto> MyAttendanceAsync() => _api.GetAsync<MyAttendanceDto>("/api/attendance/my");
}
