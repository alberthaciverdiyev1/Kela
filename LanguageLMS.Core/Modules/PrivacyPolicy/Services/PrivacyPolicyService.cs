using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.PrivacyPolicy.Requests;
using LanguageLMS.Core.Modules.PrivacyPolicy.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.PrivacyPolicy.Services;

using PrivacyPolicy = LanguageLMS.Core.Modules.PrivacyPolicy.Entities.PrivacyPolicy;

public sealed class PrivacyPolicyService(AppDbContext db) : IPrivacyPolicyService
{
    public async Task<IReadOnlyList<PrivacyPolicyResponse>> ListAsync(CancellationToken ct = default)
        => await db.PrivacyPolicies.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new PrivacyPolicyResponse(x.Type, x.Content))
            .ToListAsync(ct);

    public async Task<PrivacyPolicyResponse> GetAsync(string type, CancellationToken ct = default)
    {
        var policy = await db.PrivacyPolicies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Type == type, ct)
            ?? throw new KeyNotFoundException($"Privacy policy not found: {type}");

        return new PrivacyPolicyResponse(policy.Type, policy.Content);
    }

    public async Task<PrivacyPolicyResponse> UpsertAsync(UpsertPrivacyPolicyRequest request, CancellationToken ct = default)
    {
        var policy = await db.PrivacyPolicies.FirstOrDefaultAsync(x => x.Type == request.Type, ct);

        if (policy is null)
        {
            policy = new PrivacyPolicy { Type = request.Type, Content = request.Content };
            db.PrivacyPolicies.Add(policy);
        }
        else
        {
            policy.Content = request.Content;
        }

        await db.SaveChangesAsync(ct);
        return new PrivacyPolicyResponse(policy.Type, policy.Content);
    }
}
