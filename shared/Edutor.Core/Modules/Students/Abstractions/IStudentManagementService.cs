using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Students.Requests;
using Edutor.Core.Modules.Students.Responses;

namespace Edutor.Core.Modules.Students.Abstractions;

public interface IStudentManagementService
{
    Task<PagedResponse<StudentListItemResponse>> ListAsync(StudentListRequest request, CancellationToken ct = default);

    Task<StudentListItemResponse> GetAsync(long id, CancellationToken ct = default);

    Task<StudentListItemResponse> SetStatusAsync(long id, SetStudentStatusRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task RestoreAsync(long id, CancellationToken ct = default);


    /// <summary>Tək şagird yaratma (teacher da istifadə edir).</summary>
    Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken ct = default);

    /// <summary>Şifrəni sıfırlamaq (teacher/admin) — yeni şifrə təyin edir və ya yaradır.</summary>
    Task<ResetStudentPasswordResponse> ResetPasswordAsync(long id, ResetStudentPasswordRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<StudentDeviceResponse>> ListDevicesAsync(long studentId, CancellationToken ct = default);

    Task UnbindDeviceAsync(long studentId, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);

    /// <summary>Şagird müəllimin qruplarından ən azı birindədir.</summary>
    Task<bool> IsInTeacherGroupsAsync(long studentId, long teacherId, CancellationToken ct = default);
}
