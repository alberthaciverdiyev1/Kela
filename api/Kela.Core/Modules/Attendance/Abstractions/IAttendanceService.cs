using Kela.Core.Modules.Attendance.Requests;
using Kela.Core.Modules.Attendance.Responses;

namespace Kela.Core.Modules.Attendance.Abstractions;

public interface IAttendanceService
{
    /// <summary>Qrupun müəyyən gündəki yoxlaması: bütün üzvlər + qeyd olunubsa vəziyyəti.</summary>
    Task<AttendanceDayResponse> GetDayAsync(long groupId, DateOnly date, CancellationToken ct = default);

    /// <summary>Günün yoxlamasını saxlayır (mövcud qeydlər yenilənir, yeniləri əlavə olunur).</summary>
    Task<AttendanceDayResponse> SaveDayAsync(SaveAttendanceRequest request, CancellationToken ct = default);

    /// <summary>Ay üzrə takvim üçün gün-gün xülasə.</summary>
    Task<AttendanceMonthResponse> GetMonthAsync(long groupId, int year, int month, CancellationToken ct = default);

    /// <summary>Şagirdin öz yoxlama xülasəsi (bütün qruplar üzrə).</summary>
    Task<StudentAttendanceResponse> GetStudentSummaryAsync(long studentId, int take = 10, CancellationToken ct = default);

    /// <summary>Şagirdin bir qrup üzrə ay təqvimi (gün-gün iştirak).</summary>
    Task<StudentAttendanceMonthResponse> GetStudentMonthAsync(long studentId, long groupId, int year, int month, CancellationToken ct = default);
}
