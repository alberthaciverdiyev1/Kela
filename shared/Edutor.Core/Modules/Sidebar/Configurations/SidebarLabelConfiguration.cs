using Edutor.Core.Modules.Sidebar.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Sidebar.Configurations;

public sealed class SidebarLabelConfiguration : IEntityTypeConfiguration<SidebarLabel>
{
    public void Configure(EntityTypeBuilder<SidebarLabel> builder)
    {
        builder.ToTable("sidebar_labels");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Key).HasColumnName("key").HasMaxLength(60).IsRequired();
        builder.HasIndex(x => x.Key).IsUnique();

        builder.Property(x => x.Label).HasColumnName("label").HasMaxLength(40).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
