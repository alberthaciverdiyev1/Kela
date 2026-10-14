using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Quiz.Configurations;

using StudentQuiz = Kela.Core.Modules.Quiz.Entities.StudentQuiz;
using StudentQuizQuestion = Kela.Core.Modules.Quiz.Entities.StudentQuizQuestion;

public sealed class StudentQuizQuestionConfiguration : IEntityTypeConfiguration<StudentQuizQuestion>
{
    public void Configure(EntityTypeBuilder<StudentQuizQuestion> builder)
    {
        builder.ToTable("student_quiz_questions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.StudentQuizId).HasColumnName("student_quiz_id");
        builder.Property(x => x.QuestionId).HasColumnName("question_id");
        builder.Property(x => x.StudentAnswer).HasColumnName("student_answer").HasMaxLength(2);
        builder.Property(x => x.IsRight).HasColumnName("is_right");

        builder.HasOne(x => x.StudentQuiz).WithMany(sq => sq.Answers).HasForeignKey(x => x.StudentQuizId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Modules.Question.Entities.Question>().WithMany().HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
