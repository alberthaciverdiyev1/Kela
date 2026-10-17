using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Group.Configurations;

using Group = Kela.Core.Modules.Group.Entities.Group;
using GroupQuiz = Kela.Core.Modules.Group.Entities.GroupQuiz;
using Quiz = Kela.Core.Modules.Quiz.Entities.Quiz;

public sealed class GroupQuizConfiguration : IEntityTypeConfiguration<GroupQuiz>
{
    public void Configure(EntityTypeBuilder<GroupQuiz> builder)
    {
        builder.ToTable("group_quizzes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.QuizId).HasColumnName("quiz_id");

        builder.HasOne(x => x.Group).WithMany(g => g.Quizzes).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Quiz).WithMany().HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => new { x.GroupId, x.QuizId }).IsUnique();
    }
}
