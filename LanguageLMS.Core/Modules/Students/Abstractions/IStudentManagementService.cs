using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Students.Requests;
using LanguageLMS.Core.Modules.Students.Responses;

namespace LanguageLMS.Core.Modules.Students.Abstractions;

public interface IStudentManagementService
{
    Task<PagedResponse<StudentListItemResponse>> ListAsync(StudentListRequest request, CancellationToken ct = default);

    Task<StudentListItemResponse> GetAsync(long id, CancellationToken ct = default);

    Task<StudentListItemResponse> SetStatusAsync(long id, SetStudentStatusRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task RestoreAsync(long id, CancellationToken ct = default);

    Task<byte[]> ExportCsvAsync(StudentListRequest request, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);
}
