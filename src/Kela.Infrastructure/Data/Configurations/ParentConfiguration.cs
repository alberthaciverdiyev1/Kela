using Kela.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("parents");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);

        builder.HasOne(p => p.User)
            .WithOne(u => u.Parent)
            .HasForeignKey<Parent>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique();

        // Soft-delete: yalnızca aktif kullanıcıya ait profiller görünür
        builder.HasQueryFilter(p => p.User == null || p.User.DeletedAt == null);
    }
}
