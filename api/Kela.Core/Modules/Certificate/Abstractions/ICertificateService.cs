using Kela.Core.Common.Bulk;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Certificate.Requests;
using Kela.Core.Modules.Certificate.Responses;

namespace Kela.Core.Modules.Certificate.Abstractions;

public interface ICertificateService
{
    Task<PagedResponse<CertificateResponse>> ListAsync(CertificateListRequest request, CancellationToken ct = default);

    Task<CertificateResponse> GetAsync(long id, CancellationToken ct = default);

    Task<CertificateResponse> CreateAsync(CertificateUpsertRequest request, CancellationToken ct = default);

    Task<CertificateResponse> UpdateAsync(long id, CertificateUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task BulkAsync(BulkRequest request, CancellationToken ct = default);
}
