using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.Quiz.Configurations;

using StudentQuiz = LanguageLMS.Core.Modules.Quiz.Entities.StudentQuiz;

public sealed class StudentQuizConfiguration : IEntityTypeConfiguration<StudentQuiz>
{
    public void Configure(EntityTypeBuilder<StudentQuiz> builder)
    {
        builder.ToTable("student_quizzes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.QuizId).HasColumnName("quiz_id");
        builder.Property(x => x.AttemptTime).HasColumnName("attempt_time");
        builder.Property(x => x.TotalQuestionCount).HasColumnName("total_question_count");
        builder.Property(x => x.CompletedQuestionCount).HasColumnName("completed_question_count");
        builder.Property(x => x.IsCompleted).HasColumnName("is_completed");
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");

        builder.HasOne<Modules.Auth.Entities.Student>().WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Modules.Quiz.Entities.Quiz>().WithMany().HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => x.StudentId);
    }
}
