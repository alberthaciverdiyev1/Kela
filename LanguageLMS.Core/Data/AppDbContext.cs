using System.Linq.Expressions;
using LanguageLMS.Core.Common.Entities;
using LanguageLMS.Core.Modules.Banner.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Banner> Banners => Set<Banner>();

    public DbSet<Modules.Profession.Entities.Profession> Professions => Set<Modules.Profession.Entities.Profession>();

    public DbSet<Modules.Setting.Entities.Setting> Settings => Set<Modules.Setting.Entities.Setting>();

    public DbSet<Modules.Auth.Entities.Student> Students => Set<Modules.Auth.Entities.Student>();

    public DbSet<Modules.MainCategory.Entities.MainCategory> MainCategories => Set<Modules.MainCategory.Entities.MainCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var deletedAt = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
            var filter = Expression.Lambda(
                Expression.Equal(deletedAt, Expression.Constant(null, typeof(DateTimeOffset?))),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    private void ApplyTimestamps()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}
