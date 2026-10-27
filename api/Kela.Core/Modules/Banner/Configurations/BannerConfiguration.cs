using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Banner.Configurations;

using Banner = Kela.Core.Modules.Banner.Entities.Banner;

public sealed class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("banners");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserId).HasColumnName("user_id");

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(190)
            .IsRequired();

        builder.Property(x => x.Image)
            .HasColumnName("image")
            .HasMaxLength(500);

        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.ActiveUntil).HasColumnName("active_until").HasColumnType("date");

        builder.Property(x => x.Scope)
            .HasColumnName("scope")
            .HasMaxLength(20)
            .HasDefaultValue(Entities.BannerScope.Tenant)
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.DeletedAt);
    }
}
