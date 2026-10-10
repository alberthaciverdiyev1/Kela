using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.Question.Configurations;

using Question = LanguageLMS.Core.Modules.Question.Entities.Question;
using Subject = LanguageLMS.Core.Modules.Subject.Entities.Subject;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.SubjectId).HasColumnName("subject_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.VariantA).HasColumnName("variant_a").HasMaxLength(1000);
        builder.Property(x => x.VariantB).HasColumnName("variant_b").HasMaxLength(1000);
        builder.Property(x => x.VariantC).HasColumnName("variant_c").HasMaxLength(1000);
        builder.Property(x => x.VariantD).HasColumnName("variant_d").HasMaxLength(1000);
        builder.Property(x => x.VariantE).HasColumnName("variant_e").HasMaxLength(1000);
        builder.Property(x => x.CorrectAnswer).HasColumnName("correct_answer").HasMaxLength(2).IsRequired();
        builder.Property(x => x.VariantType).HasColumnName("variant_type").HasMaxLength(20).IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id");

        builder.HasOne<Subject>().WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => x.SubjectId);
    }
}
