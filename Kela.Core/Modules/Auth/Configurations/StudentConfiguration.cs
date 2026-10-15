using Kela.Core.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Auth.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(190).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();

        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(x => x.PasswordHash).HasColumnName("password").IsRequired();
        builder.Property(x => x.Image).HasColumnName("image").HasMaxLength(500);
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.NotificationStatus).HasColumnName("notification_status");
        builder.Property(x => x.MustChangePassword).HasColumnName("must_change_password");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
