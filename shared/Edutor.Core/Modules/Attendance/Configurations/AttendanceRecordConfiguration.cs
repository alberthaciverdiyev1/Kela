using Edutor.Core.Modules.Attendance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Attendance.Configurations;

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_records");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.Date).HasColumnName("date");
        builder.Property(x => x.IsPresent).HasColumnName("is_present");
        builder.Property(x => x.Point).HasColumnName("point");
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(500);

        builder.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => new { x.GroupId, x.Date });
        builder.HasIndex(x => new { x.GroupId, x.StudentId, x.Date }).IsUnique();
    }
}
