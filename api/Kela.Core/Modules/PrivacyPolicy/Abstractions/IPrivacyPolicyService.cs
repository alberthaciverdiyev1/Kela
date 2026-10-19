using Kela.Core.Modules.PrivacyPolicy.Requests;
using Kela.Core.Modules.PrivacyPolicy.Responses;

namespace Kela.Core.Modules.PrivacyPolicy.Abstractions;

public interface IPrivacyPolicyService
{
    Task<IReadOnlyList<PrivacyPolicyResponse>> ListAsync(CancellationToken ct = default);

    Task<PrivacyPolicyResponse> GetAsync(string type, CancellationToken ct = default);

    /// <summary>Tenant konteksti olmadan oxunur (public qeydiyyat səhifəsi üçün).</summary>
    Task<PrivacyPolicyResponse?> GetPublicAsync(string type, CancellationToken ct = default);

    Task<PrivacyPolicyResponse> UpsertAsync(UpsertPrivacyPolicyRequest request, CancellationToken ct = default);
}
