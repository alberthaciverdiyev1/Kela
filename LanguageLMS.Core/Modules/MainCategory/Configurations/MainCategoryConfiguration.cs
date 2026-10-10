using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.MainCategory.Configurations;

using MainCategory = LanguageLMS.Core.Modules.MainCategory.Entities.MainCategory;

public sealed class MainCategoryConfiguration : IEntityTypeConfiguration<MainCategory>
{
    public void Configure(EntityTypeBuilder<MainCategory> builder)
    {
        builder.ToTable("main_categories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(190)
            .IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.UserId).HasColumnName("user_id");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
