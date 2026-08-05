using Kela.Domain.Attendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("attendances");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(a => a.Status).HasConversion<int>().IsRequired();

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);

        builder.HasOne(a => a.Lesson)
            .WithMany(l => l.Attendances)
            .HasForeignKey(a => a.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Aynı öğrenci aynı ders oturumunda yalnızca bir kez
        builder.HasIndex(a => new { a.LessonId, a.StudentId }).IsUnique();

        builder.HasQueryFilter(a => a.DeletedAt == null);
    }
}
