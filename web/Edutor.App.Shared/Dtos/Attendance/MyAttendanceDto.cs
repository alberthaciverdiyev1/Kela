namespace Edutor.App.Shared.Dtos.Attendance;

public record MyAttendanceItemDto(DateOnly Date, string GroupName, bool IsPresent);

public record MyAttendanceDto(
    int TotalCount,
    int PresentCount,
    int AbsentCount,
    int Percent,
    List<MyAttendanceItemDto> Recent);
