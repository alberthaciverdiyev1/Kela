using Edutor.Core.Data;
using Edutor.Core.Modules.TenantConfig.Abstractions;
using Edutor.Core.Modules.TenantConfig.Entities;
using Edutor.Core.Modules.TenantConfig.Requests;
using Edutor.Core.Modules.TenantConfig.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.TenantConfig.Services;

public sealed class TenantConfigService(AppDbContext db) : ITenantConfigService
{
    public async Task<TenantConfigResponse> GetAsync(CancellationToken ct = default)
    {
        var row = await db.TenantConfigs.AsNoTracking().FirstOrDefaultAsync(ct);
        return row is null
            ? new TenantConfigResponse(PaymentBlockModes.None, [])
            : ToResponse(row);
    }

    public async Task<TenantConfigResponse> UpdateAsync(TenantConfigUpsertRequest request, CancellationToken ct = default)
    {
        var row = await db.TenantConfigs.FirstOrDefaultAsync(ct);
        if (row is null)
        {
            row = new Entities.TenantConfig();
            db.TenantConfigs.Add(row);
        }

        row.PaymentBlockMode = request.PaymentBlockMode;
        row.PaymentBlockedGroupIds = request.PaymentBlockMode == PaymentBlockModes.Groups
                                     && request.PaymentBlockedGroupIds.Count > 0
            ? string.Join(',', request.PaymentBlockedGroupIds.Distinct())
            : null;

        await db.SaveChangesAsync(ct);
        return ToResponse(row);
    }

    private static TenantConfigResponse ToResponse(Entities.TenantConfig row) =>
        new(row.PaymentBlockMode, ParseIds(row.PaymentBlockedGroupIds));

    public static IReadOnlyList<long> ParseIds(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => long.TryParse(x, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
}
