using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Feedback.Configurations;

using Feedback = Kela.Core.Modules.Feedback.Entities.Feedback;
using FeedbackMessage = Kela.Core.Modules.Feedback.Entities.FeedbackMessage;

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
        builder.Property(x => x.LastMessageAt).HasColumnName("last_message_at");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.FeedbackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.LastMessageAt);
        builder.HasIndex(x => x.DeletedAt);
    }
}

public sealed class FeedbackMessageConfiguration : IEntityTypeConfiguration<FeedbackMessage>
{
    public void Configure(EntityTypeBuilder<FeedbackMessage> builder)
    {
        builder.ToTable("feedback_messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FeedbackId).HasColumnName("feedback_id");
        builder.Property(x => x.SenderRole).HasColumnName("sender_role").HasMaxLength(16).IsRequired();
        builder.Property(x => x.SenderName).HasColumnName("sender_name").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Body).HasColumnName("body").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Image).HasColumnName("image").HasMaxLength(500);
        builder.Property(x => x.IsRead).HasColumnName("is_read");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.FeedbackId, x.CreatedAt });
    }
}
