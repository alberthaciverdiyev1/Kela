using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(l => l.ContentId);
        builder.Property(l => l.ContentId).ValueGeneratedNever();

        builder.Property(l => l.VideoPath).HasMaxLength(500);
        builder.Property(l => l.ThumbnailPath).HasMaxLength(500);
        builder.Property(l => l.DurationSeconds).IsRequired();
        builder.Property(l => l.IsPublished).IsRequired();
        builder.Property(l => l.OrderIndex).IsRequired();

        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt);

        builder.HasOne(l => l.Content)
            .WithOne(c => c.Lesson)
            .HasForeignKey<Lesson>(l => l.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Teacher)
            .WithMany()
            .HasForeignKey(l => l.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.TeacherId);
    }
}
