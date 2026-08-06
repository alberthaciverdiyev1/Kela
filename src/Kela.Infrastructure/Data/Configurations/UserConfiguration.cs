using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

/// <summary>
/// Yalnızca User'a özgü alanları yapılandırır.
/// Identity kolonları (Email, NormalizedEmail, PasswordHash, vb.) ve AspNet* tabloları
/// IdentityDbContext tarafından kurulur (tablo adı "AspNetUsers") — burada tekrarlanmaz.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.Status).HasConversion<int>().IsRequired();

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.DeletedAt);

        builder.HasIndex(u => u.Status);

        // Soft-delete global query filter
        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
