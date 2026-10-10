using Microsoft.AspNetCore.Http;

namespace LanguageLMS.Core.Common.MultiTenancy;

/// <summary>
/// Tenant: əvvəlcə "X-Tenant-Id" header-ı (admin tenant dəyişməsi / bootstrap), yoxsa JWT-dəki "tenant_id" claim-i.
/// </summary>
public sealed class HttpTenantProvider(IHttpContextAccessor accessor) : ITenantProvider
{
    public long? TenantId
    {
        get
        {
            var context = accessor.HttpContext;
            if (context is null) return null;

            var header = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (long.TryParse(header, out var headerId)) return headerId;

            var claim = context.User?.FindFirst("tenant_id")?.Value;
            if (long.TryParse(claim, out var claimId)) return claimId;

            return null;
        }
    }
}
