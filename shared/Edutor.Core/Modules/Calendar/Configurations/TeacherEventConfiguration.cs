using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Calendar.Configurations;

using TeacherEvent = Edutor.Core.Modules.Calendar.Entities.TeacherEvent;

public sealed class TeacherEventConfiguration : IEntityTypeConfiguration<TeacherEvent>
{
    public void Configure(EntityTypeBuilder<TeacherEvent> builder)
    {
        builder.ToTable("teacher_events");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(x => x.StartAt).HasColumnName("start_at");
        builder.Property(x => x.EndAt).HasColumnName("end_at");
        builder.Property(x => x.Color).HasColumnName("color").HasMaxLength(20).IsRequired().HasDefaultValue("default");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => new { x.UserId, x.StartAt });

        builder.HasOne<Edutor.Core.Modules.User.Entities.User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
