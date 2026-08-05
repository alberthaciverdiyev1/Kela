using System.Linq.Expressions;
using System.Reflection;
using Kela.Application.Abstractions.Tenancy;
using Kela.Application.Repositories;
using Kela.Domain.Attendances;
using Kela.Domain.Common;
using Kela.Domain.Sections;
using Kela.Domain.Lessons;
using Kela.Domain.Payments;
using Kela.Domain.Subjects;
using Kela.Domain.Tenants;
using Kela.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Data;

public sealed class KelaDbContext(
    DbContextOptions<KelaDbContext> options,
    ICurrentTenant? currentTenant = null)
    : DbContext(options), IUnitOfWork
{
    // EF Core, query filter'daki field erişimini context instance'a bağlar.
    // Tenant çözümlenmemişse null kalır → FAIL-CLOSED: hiçbir kayıt görünmez (veri sızıntısı önlendi).
    // Middleware (TenantResolutionMiddleware) SetTenantId ile set eder.
    private int? _tenantId = currentTenant?.IsResolved == true ? currentTenant.TenantId : null;

    /// <summary>
    /// Tenant çözümleme middleware'i tarafından çağrılır.
    /// Sonraki sorgular bu tenant kapsamında çalışır.
    /// </summary>
    public void SetTenantId(int tenantId) => _tenantId = tenantId;

    public DbSet<User> Users => Set<User>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<PaymentTrack> PaymentTracks => Set<PaymentTrack>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KelaDbContext).Assembly);

        ApplyTenantFilters(modelBuilder);
        ApplyTenantIndexes(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Tüm ITenantEntity tiplerine tenant global query filter'ı uygular.
    /// Mevcut (soft-delete vb.) filter'lar AND ile korunur.
    /// </summary>
    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        var method = typeof(KelaDbContext)
            .GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            method.MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        var builder = modelBuilder.Entity<TEntity>();
        var existing = builder.Metadata.GetDeclaredQueryFilters().SingleOrDefault()?.Expression;

        // FAIL-CLOSED: _tenantId == null (çözümlenmedi) → hiçbir kayıt görünmez.
        // _tenantId != null → yalnızca o tenant'ın kayıtları görünür.
        // (Eskiden null → tüm kayıtlar görünürdü; bu veri sızıntısına yol açardı.)
        Expression<Func<TEntity, bool>> tenantFilter = e => _tenantId != null && e.TenantId == _tenantId;

        if (existing is null)
        {
            builder.HasQueryFilter(tenantFilter);
            return;
        }

        var parameter = tenantFilter.Parameters[0];
        var existingBody = new ReplaceParameterVisitor(existing.Parameters[0], parameter).Visit(existing.Body);
        var combined = Expression.AndAlso(existingBody, tenantFilter.Body);

        builder.HasQueryFilter(Expression.Lambda(combined, parameter));
    }

    /// <summary>
    /// Tüm tenant entity'lerine TenantId index'i ekler (tenant-bazlı sorgular için).
    /// </summary>
    private static void ApplyTenantIndexes(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType).HasIndex("TenantId");
        }
    }

    private sealed class ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == source ? target : base.VisitParameter(node);
    }
}
