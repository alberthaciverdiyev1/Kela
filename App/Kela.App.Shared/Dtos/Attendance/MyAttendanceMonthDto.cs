namespace Kela.App.Shared.Dtos.Attendance;

public record MyAttendanceDayDto(DateOnly Date, bool IsPresent);

public record MyAttendanceMonthDto(
    long GroupId,
    int Year,
    int Month,
    int PresentCount,
    int TotalCount,
    List<MyAttendanceDayDto> Days);
