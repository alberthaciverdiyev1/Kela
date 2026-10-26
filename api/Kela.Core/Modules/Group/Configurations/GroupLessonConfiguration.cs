using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Group.Configurations;

using Group = Kela.Core.Modules.Group.Entities.Group;
using GroupLesson = Kela.Core.Modules.Group.Entities.GroupLesson;
using Lesson = Kela.Core.Modules.Lesson.Entities.Lesson;

public sealed class GroupLessonConfiguration : IEntityTypeConfiguration<GroupLesson>
{
    public void Configure(EntityTypeBuilder<GroupLesson> builder)
    {
        builder.ToTable("group_lessons");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.LessonId).HasColumnName("lesson_id");

        builder.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Lesson).WithMany().HasForeignKey(x => x.LessonId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => new { x.GroupId, x.LessonId }).IsUnique();
    }
}
