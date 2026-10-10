using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.SystemNotification.Configurations;

using SystemNotification = LanguageLMS.Core.Modules.SystemNotification.Entities.SystemNotification;

public sealed class SystemNotificationConfiguration : IEntityTypeConfiguration<SystemNotification>
{
    public void Configure(EntityTypeBuilder<SystemNotification> builder)
    {
        builder.ToTable("system_notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RecipientId).HasColumnName("recipient_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Body).HasColumnName("body");
        builder.Property(x => x.Data).HasColumnName("data");
        builder.Property(x => x.ReadAt).HasColumnName("read_at");
        builder.HasOne<Modules.User.Entities.User>().WithMany().HasForeignKey(x => x.RecipientId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.RecipientId, x.ReadAt });
    }
}
