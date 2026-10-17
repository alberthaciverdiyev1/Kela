using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Feedback.Configurations;

using Feedback = Kela.Core.Modules.Feedback.Entities.Feedback;

public sealed class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("feedbacks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(190).IsRequired();
        builder.Property(x => x.UserEmail).HasColumnName("user_email").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Image).HasColumnName("image").HasMaxLength(500);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(16).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.DeletedAt);
    }
}
