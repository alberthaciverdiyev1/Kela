using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.TenantConfig.Configurations;

using TenantConfig = Edutor.Core.Modules.TenantConfig.Entities.TenantConfig;

public sealed class TenantConfigConfiguration : IEntityTypeConfiguration<TenantConfig>
{
    public void Configure(EntityTypeBuilder<TenantConfig> builder)
    {
        builder.ToTable("tenant_configs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.PaymentBlockMode).HasColumnName("payment_block_mode").HasMaxLength(20).IsRequired();
        builder.Property(x => x.PaymentBlockedGroupIds).HasColumnName("payment_blocked_group_ids").HasMaxLength(2000);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.TenantId, x.DeletedAt });
    }
}
