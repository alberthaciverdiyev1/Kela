using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.Notification.Configurations;

using Notification = LanguageLMS.Core.Modules.Notification.Entities.Notification;
using NotificationStudent = LanguageLMS.Core.Modules.Notification.Entities.NotificationStudent;

public sealed class NotificationStudentConfiguration : IEntityTypeConfiguration<NotificationStudent>
{
    public void Configure(EntityTypeBuilder<NotificationStudent> builder)
    {
        builder.ToTable("notification_students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.NotificationId).HasColumnName("notification_id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");

        builder.HasOne(x => x.Notification).WithMany(n => n.Recipients).HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Modules.Auth.Entities.Student>().WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.NotificationId, x.StudentId });
    }
}
