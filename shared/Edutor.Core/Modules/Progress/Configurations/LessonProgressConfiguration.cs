using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Progress.Configurations;

using LessonProgress = Edutor.Core.Modules.Progress.Entities.LessonProgress;

public sealed class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.ToTable("lesson_progress");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.LessonContentId).HasColumnName("lesson_content_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");

        builder.HasOne<Modules.Auth.Entities.Student>().WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Modules.LessonContent.Entities.LessonContent>().WithMany().HasForeignKey(x => x.LessonContentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => new { x.StudentId, x.LessonContentId }).IsUnique();
    }
}
