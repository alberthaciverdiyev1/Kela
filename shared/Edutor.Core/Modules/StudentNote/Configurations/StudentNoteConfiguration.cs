using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.StudentNote.Configurations;

using StudentNote = Edutor.Core.Modules.StudentNote.Entities.StudentNote;

public sealed class StudentNoteConfiguration : IEntityTypeConfiguration<StudentNote>
{
    public void Configure(EntityTypeBuilder<StudentNote> builder)
    {
        builder.ToTable("student_notes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.LessonId).HasColumnName("lesson_id");
        builder.Property(x => x.LessonContentId).HasColumnName("lesson_content_id");
        builder.Property(x => x.PositionSeconds).HasColumnName("position_seconds");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190);
        builder.Property(x => x.Body).HasColumnName("body").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Visibility).HasColumnName("visibility").HasMaxLength(16).IsRequired();
        builder.Property(x => x.GroupId).HasColumnName("group_id");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.LessonContentId);
        builder.HasIndex(x => new { x.Visibility, x.GroupId });
        builder.HasIndex(x => x.DeletedAt);

        builder.HasOne<Edutor.Core.Modules.Auth.Entities.Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Edutor.Core.Modules.Lesson.Entities.Lesson>()
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Edutor.Core.Modules.LessonContent.Entities.LessonContent>()
            .WithMany()
            .HasForeignKey(x => x.LessonContentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Edutor.Core.Modules.Group.Entities.Group>()
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
