using Kela.Core.Data;
using Kela.Core.Common.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Common.Storage;

public sealed class TenantFolder(AppDbContext db, ITenantProvider tenants) : ITenantFolder
{
    public async Task<string> ResolveAsync(long? fallbackTenantId = null, CancellationToken ct = default)
    {
        var id = tenants.TenantId is > 0 ? tenants.TenantId : fallbackTenantId;
        if (id is not > 0) return "shared";

        var tenant = await db.Tenants.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new { t.Name, t.Slug })
            .FirstOrDefaultAsync(ct);

        if (tenant is null) return $"tenant-{id}";

        if (!string.IsNullOrWhiteSpace(tenant.Slug)) return Sanitize(tenant.Slug);

        var fromName = Sanitize(tenant.Name);
        return fromName.Length > 0 ? fromName : $"tenant-{id}";
    }

    /// <summary>
    /// Qovluq adı üçün yalnız ASCII hərf/rəqəm saxlanılır — tenant adlarında boşluq
    /// və Azərbaycan hərfləri ola bilər, bucket açarlarında isə URL-təhlükəsizlik lazımdır.
    /// </summary>
    private static string Sanitize(string value)
    {
        var lowered = value.Trim().ToLowerInvariant();
        var sb = new System.Text.StringBuilder(lowered.Length);

        foreach (var ch in lowered)
        {
            sb.Append(ch is >= 'a' and <= 'z' or >= '0' and <= '9' ? ch : '-');
        }

        var result = sb.ToString().Trim('-');
        while (result.Contains("--")) result = result.Replace("--", "-");
        return result;
    }
}
