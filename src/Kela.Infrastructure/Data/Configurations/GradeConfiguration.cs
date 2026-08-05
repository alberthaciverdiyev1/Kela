using Kela.Domain.Grades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("grades");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(g => g.Name).HasMaxLength(50).IsRequired();
        builder.Property(g => g.Level).IsRequired();

        builder.Property(g => g.CreatedAt).IsRequired();
        builder.Property(g => g.UpdatedAt);

        builder.HasOne(g => g.Teacher)
            .WithMany()
            .HasForeignKey(g => g.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(g => g.Students)
            .WithMany(s => s.Grades);

        builder.HasIndex(g => g.Name).IsUnique();
    }
}
