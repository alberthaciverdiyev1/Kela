using Kela.Domain.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(l => l.Title).HasMaxLength(200).IsRequired();
        builder.Property(l => l.ScheduledAt).IsRequired();
        builder.Property(l => l.DurationMinutes).IsRequired();

        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt);

        builder.HasOne(l => l.Subject)
            .WithMany()
            .HasForeignKey(l => l.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Teacher)
            .WithMany()
            .HasForeignKey(l => l.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Aynı gün aynı ders için aranabilir
        builder.HasIndex(l => new { l.SubjectId, l.ScheduledAt });

        builder.HasQueryFilter(l => l.DeletedAt == null);
    }
}
