using System.Security.Claims;
using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Core.Modules.User.Entities;

namespace Edutor.Admin.Infrastructure;

/// <summary>Abunəlik və plan səhifələri üçün ortaq tenant seçimi və WhatsApp nömrəsi.</summary>
public static class TenantPageContext
{
    private const string DefaultWhatsApp = "994709990569";

    /// <summary>Superadmin üçün seçilmiş tenant: query → keçmiş tenant claim → ilk tenant.</summary>
    public static async Task<long?> ResolveAsync(
        ClaimsPrincipal user, long? requestedTenantId, ITenantService tenants, CancellationToken ct)
    {
        if (!user.IsInRole(UserTypes.SuperAdmin))
        {
            var own = user.CurrentTenantId();
            return own > 0 ? own : null;
        }

        if (requestedTenantId is > 0)
        {
            return await tenants.GetByIdAsync(requestedTenantId.Value, ct) is not null ? requestedTenantId : null;
        }

        var switched = user.CurrentTenantId();
        if (switched > 0) return switched;

        var all = await tenants.ListAsync(ct);
        return all.FirstOrDefault()?.Id;
    }

    public static string NormalizeWhatsApp(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digits.StartsWith("00")) digits = digits[2..];
        if (digits.Length == 0) return DefaultWhatsApp;

        // Yerli format (0XX XXX XX XX) → beynəlxalq (994XX...).
        if (digits.Length == 10 && digits.StartsWith('0')) digits = "994" + digits[1..];

        return digits;
    }
}
