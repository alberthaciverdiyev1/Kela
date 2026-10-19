using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Tenant.Configurations;

using Tenant = Kela.Core.Modules.Tenant.Entities.Tenant;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.IsPending).HasColumnName("is_pending").HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
