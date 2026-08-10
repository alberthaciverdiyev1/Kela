using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(q => q.Text).HasMaxLength(2000).IsRequired();
        builder.Property(q => q.OptionA).HasMaxLength(500).IsRequired();
        builder.Property(q => q.OptionB).HasMaxLength(500).IsRequired();
        builder.Property(q => q.OptionC).HasMaxLength(500).IsRequired();
        builder.Property(q => q.OptionD).HasMaxLength(500);
        builder.Property(q => q.OptionE).HasMaxLength(500);
        builder.Property(q => q.CorrectOption).IsRequired();

        builder.Property(q => q.CreatedAt).IsRequired();
        builder.Property(q => q.UpdatedAt);

        builder.HasOne(q => q.Teacher)
            .WithMany()
            .HasForeignKey(q => q.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(q => q.DeletedAt == null);

        builder.HasIndex(q => new { q.TeacherId, q.CreatedAt });
    }
}
