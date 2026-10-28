using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Quiz.Configurations;

using Quiz = Edutor.Core.Modules.Quiz.Entities.Quiz;
using Lesson = Edutor.Core.Modules.Lesson.Entities.Lesson;

public sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("quizzes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.LessonId).HasColumnName("lesson_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190).IsRequired();
        builder.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.IsRepeat).HasColumnName("is_repeat");
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.Property(x => x.UserId).HasColumnName("user_id");

        builder.HasOne(x => x.Lesson).WithMany().HasForeignKey(x => x.LessonId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => x.LessonId);
    }
}
