using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.Profession.Configurations;

using Profession = LanguageLMS.Core.Modules.Profession.Entities.Profession;

public sealed class ProfessionConfiguration : IEntityTypeConfiguration<Profession>
{
    public void Configure(EntityTypeBuilder<Profession> builder)
    {
        builder.ToTable("professions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(190)
            .IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Image).HasColumnName("image").HasMaxLength(500);
        builder.Property(x => x.ParentId).HasColumnName("parent_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.DeletedAt);
    }
}
