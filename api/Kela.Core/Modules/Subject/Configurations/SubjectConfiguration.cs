using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Subject.Configurations;

using Subject = Kela.Core.Modules.Subject.Entities.Subject;

public sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(190)
            .IsRequired();
        // Ad yalnız öz tenant-ında unikal olmalıdır; silinmiş fənn adı yenidən
        // istifadə oluna bilsin deyə indeks soft-delete sətirlərini əhatə etmir.
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.IsShared).HasColumnName("is_shared").HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
