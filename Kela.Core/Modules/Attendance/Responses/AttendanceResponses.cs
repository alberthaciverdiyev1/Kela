namespace Kela.Core.Modules.Attendance.Responses;

public record AttendanceStudentItem(long StudentId, string FullName, string Email, bool IsPresent);

public record AttendanceDayResponse(
    long GroupId,
    DateOnly Date,
    int PresentCount,
    int TotalCount,
    List<AttendanceStudentItem> Students);

public record AttendanceDayStat(DateOnly Date, int PresentCount, int TotalCount);

public record AttendanceMonthResponse(long GroupId, int Year, int Month, List<AttendanceDayStat> Days);

public record StudentAttendanceItem(DateOnly Date, string GroupName, bool IsPresent);

public record StudentAttendanceResponse(
    int TotalCount,
    int PresentCount,
    int AbsentCount,
    int Percent,
    List<StudentAttendanceItem> Recent);

public record StudentAttendanceDayItem(DateOnly Date, bool IsPresent);

public record StudentAttendanceMonthResponse(
    long GroupId,
    int Year,
    int Month,
    int PresentCount,
    int TotalCount,
    List<StudentAttendanceDayItem> Days);
