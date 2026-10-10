using LanguageLMS.Core.Modules.PrivacyPolicy.Requests;
using LanguageLMS.Core.Modules.PrivacyPolicy.Responses;

namespace LanguageLMS.Core.Modules.PrivacyPolicy.Services;

public interface IPrivacyPolicyService
{
    Task<IReadOnlyList<PrivacyPolicyResponse>> ListAsync(CancellationToken ct = default);

    Task<PrivacyPolicyResponse> GetAsync(string type, CancellationToken ct = default);

    Task<PrivacyPolicyResponse> UpsertAsync(UpsertPrivacyPolicyRequest request, CancellationToken ct = default);
}
