using Kela.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(100).IsRequired();

        builder.Property(t => t.Status).HasConversion<int>().IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt);
        builder.Property(t => t.DeletedAt);

        // Slug, subdomain/header çözümlemesinde kullanılır → global unique
        builder.HasIndex(t => new { t.Slug, t.DeletedAt }).IsUnique();

        builder.HasQueryFilter(t => t.DeletedAt == null);
    }
}
