namespace Kela.Core.Modules.Attendance.Responses;

/// <summary>IsRecorded: bu gün üçün qeyd varmı. IsPresent yalnız qeyd varsa mənalıdır —
/// beləcə "qayıb" ilə "qeyd olunmayıb" bir-birindən ayrılır.</summary>
public record AttendanceStudentItem(long StudentId, string FullName, string Email, bool IsPresent, bool IsRecorded);

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
