using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.LessonContent.Configurations;

using LessonContent = Edutor.Core.Modules.LessonContent.Entities.LessonContent;
using LessonContentLesson = Edutor.Core.Modules.LessonContent.Entities.LessonContentLesson;
using Lesson = Edutor.Core.Modules.Lesson.Entities.Lesson;

public sealed class LessonContentLessonConfiguration : IEntityTypeConfiguration<LessonContentLesson>
{
    public void Configure(EntityTypeBuilder<LessonContentLesson> builder)
    {
        builder.ToTable("lesson_content_lessons");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.LessonContentId).HasColumnName("lesson_content_id");
        builder.Property(x => x.LessonId).HasColumnName("lesson_id");
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");

        builder.HasOne(x => x.Content).WithMany(c => c.AdditionalLessons)
            .HasForeignKey(x => x.LessonContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Lesson).WithMany()
            .HasForeignKey(x => x.LessonId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.LessonContentId, x.LessonId }).IsUnique();
        builder.HasIndex(x => x.LessonId);
        builder.HasIndex(x => x.DeletedAt);
    }
}
