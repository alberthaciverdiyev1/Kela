using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Students.Requests;
using Kela.Core.Modules.Students.Responses;

namespace Kela.Core.Modules.Students.Abstractions;

public interface IStudentManagementService
{
    Task<PagedResponse<StudentListItemResponse>> ListAsync(StudentListRequest request, CancellationToken ct = default);

    Task<StudentListItemResponse> GetAsync(long id, CancellationToken ct = default);

    Task<StudentListItemResponse> SetStatusAsync(long id, SetStudentStatusRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task RestoreAsync(long id, CancellationToken ct = default);

    Task<byte[]> ExportXlsxAsync(StudentListRequest request, CancellationToken ct = default);

    Task<GenerateStudentsResponse> GenerateAsync(GenerateStudentsRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<StudentDeviceResponse>> ListDevicesAsync(long studentId, CancellationToken ct = default);

    Task UnbindDeviceAsync(long studentId, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);
}
