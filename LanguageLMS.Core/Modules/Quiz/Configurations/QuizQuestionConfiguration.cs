using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.Quiz.Configurations;

using Quiz = LanguageLMS.Core.Modules.Quiz.Entities.Quiz;
using QuizQuestion = LanguageLMS.Core.Modules.Quiz.Entities.QuizQuestion;
using Question = LanguageLMS.Core.Modules.Question.Entities.Question;

public sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("quiz_questions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.QuizId).HasColumnName("quiz_id");
        builder.Property(x => x.QuestionId).HasColumnName("question_id");
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");

        builder.HasOne(x => x.Quiz).WithMany(q => q.Questions).HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Question>().WithMany().HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => new { x.QuizId, x.QuestionId }).IsUnique();
    }
}
