using Kela.Application.Abstractions.Tenancy;
using Kela.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kela.Infrastructure.Tenancy;

/// <summary>
/// Eklenen ve değiştirilen tenant entity'lerine TenantId'yi otomatik yazar.
/// Böylece handler'lar TenantId'yi elle set etmek zorunda kalmaz.
/// Tenant çözümlenmemişse yazım yapılmaz (Faz 2 fail-closed sonrası zaten çözümlenecek).
/// </summary>
internal sealed class TenantSaveChangesInterceptor(ICurrentTenant currentTenant)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetTenantId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetTenantId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetTenantId(DbContext? context)
    {
        if (context is null || !currentTenant.IsResolved)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not ITenantEntity tenantEntity)
            {
                continue;
            }

            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            tenantEntity.TenantId = currentTenant.TenantId;
        }
    }
}
