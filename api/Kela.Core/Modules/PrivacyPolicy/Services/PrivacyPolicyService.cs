using Kela.Core.Common.Content;
using Kela.Core.Data;
using Kela.Core.Modules.PrivacyPolicy.Requests;
using Kela.Core.Modules.PrivacyPolicy.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.PrivacyPolicy.Services;

using PrivacyPolicy = Kela.Core.Modules.PrivacyPolicy.Entities.PrivacyPolicy;

public sealed class PrivacyPolicyService(AppDbContext db) : IPrivacyPolicyService
{
    public async Task<IReadOnlyList<PrivacyPolicyResponse>> ListAsync(CancellationToken ct = default)
    {
        var items = await db.PrivacyPolicies.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
        return items.Select(x => new PrivacyPolicyResponse(x.Type, SafeHtml.Sanitize(x.Content) ?? string.Empty)).ToList();
    }

    public async Task<PrivacyPolicyResponse> GetAsync(string type, CancellationToken ct = default)
    {
        var policy = await db.PrivacyPolicies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Type == type, ct)
            ?? throw new KeyNotFoundException($"Privacy policy not found: {type}");

        return new PrivacyPolicyResponse(policy.Type, SafeHtml.Sanitize(policy.Content) ?? string.Empty);
    }

    public async Task<PrivacyPolicyResponse?> GetPublicAsync(string type, CancellationToken ct = default)
    {
        var policy = await db.PrivacyPolicies
            .IgnoreQueryFilters(new[] { "Tenant" })
            .AsNoTracking()
            .Where(x => x.Type == type)
            .OrderBy(x => x.TenantId == 0 ? 0 : 1)
            .ThenBy(x => x.TenantId)
            .FirstOrDefaultAsync(ct);

        return policy is null ? null : new PrivacyPolicyResponse(policy.Type, SafeHtml.Sanitize(policy.Content) ?? string.Empty);
    }

    public async Task<PrivacyPolicyResponse> UpsertAsync(UpsertPrivacyPolicyRequest request, CancellationToken ct = default)
    {
        var content = SafeHtml.Sanitize(request.Content) ?? string.Empty;
        var policy = await db.PrivacyPolicies.FirstOrDefaultAsync(x => x.Type == request.Type, ct);

        if (policy is null)
        {
            policy = new PrivacyPolicy { Type = request.Type, Content = content };
            db.PrivacyPolicies.Add(policy);
        }
        else
        {
            policy.Content = content;
        }

        await db.SaveChangesAsync(ct);
        return new PrivacyPolicyResponse(policy.Type, policy.Content);
    }
}
