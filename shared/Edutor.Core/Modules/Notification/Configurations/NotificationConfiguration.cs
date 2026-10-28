using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Notification.Configurations;

using Notification = Edutor.Core.Modules.Notification.Entities.Notification;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Image).HasColumnName("image").HasMaxLength(500);
        builder.Property(x => x.Body).HasColumnName("body");
        builder.Property(x => x.All).HasColumnName("all").HasDefaultValue(true);
        builder.Property(x => x.Data).HasColumnName("data");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
