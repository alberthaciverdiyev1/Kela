using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.User.Configurations;

using User = LanguageLMS.Core.Modules.User.Entities.User;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(190);
        builder.Property(x => x.Surname).HasColumnName("surname").HasMaxLength(190);

        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(190).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash).HasColumnName("password").IsRequired();
        builder.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(50);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
