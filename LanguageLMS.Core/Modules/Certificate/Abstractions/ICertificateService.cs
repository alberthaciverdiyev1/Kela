using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Certificate.Requests;
using LanguageLMS.Core.Modules.Certificate.Responses;

namespace LanguageLMS.Core.Modules.Certificate.Abstractions;

public interface ICertificateService
{
    Task<PagedResponse<CertificateResponse>> ListAsync(CertificateListRequest request, CancellationToken ct = default);

    Task<CertificateResponse> GetAsync(long id, CancellationToken ct = default);

    Task<CertificateResponse> CreateAsync(CertificateUpsertRequest request, CancellationToken ct = default);

    Task<CertificateResponse> UpdateAsync(long id, CertificateUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);
}
