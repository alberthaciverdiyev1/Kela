using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("quizzes");

        builder.HasKey(q => q.ContentId);
        builder.Property(q => q.ContentId).ValueGeneratedNever();

        builder.Property(q => q.Title).HasMaxLength(200).IsRequired();
        builder.Property(q => q.Description).HasMaxLength(2000);
        builder.Property(q => q.IsPublished).IsRequired();

        builder.Property(q => q.CreatedAt).IsRequired();
        builder.Property(q => q.UpdatedAt);

        builder.HasOne(q => q.Content)
            .WithOne()
            .HasForeignKey<Quiz>(q => q.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.Teacher)
            .WithMany()
            .HasForeignKey(q => q.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.TeacherId);
    }
}
