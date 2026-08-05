using Kela.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");

        // Shared primary key: profil kendi Id'sini taşımaz, User.Id'yi anahtar yapar.
        builder.HasKey(t => t.UserId);
        builder.Property(t => t.UserId).ValueGeneratedNever();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt);

        builder.HasOne(t => t.User)
            .WithOne(u => u.Teacher)
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft-delete: yalnızca aktif kullanıcıya ait profiller görünür
        builder.HasQueryFilter(t => t.User == null || t.User.DeletedAt == null);
    }
}
