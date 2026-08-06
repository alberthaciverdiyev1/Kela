using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .UseIdentityColumn()
            .ValueGeneratedOnAdd();

        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        // C# adı PasswordHash; DB kolonu mevcut "Password" olarak korunur
        // (migration'da drop+add yerine veri korunur). İleride RenameColumn ile düzeltilebilir.
        builder.Property(u => u.PasswordHash).HasColumnName("Password").HasMaxLength(500).IsRequired();

        builder.Property(u => u.Role).HasConversion<int>().IsRequired();
        builder.Property(u => u.Status).HasConversion<int>().IsRequired();

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.DeletedAt);

        // Soft-delete: eşsiz e-posta (silinenler hariç)
        builder.HasIndex(u => new { u.Email, u.DeletedAt }).IsUnique();
        builder.HasIndex(u => new { u.Role, u.Status });

        // Soft-delete global query filter
        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
