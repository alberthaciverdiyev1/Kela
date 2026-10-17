using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Auth.Configurations;

using StudentDevice = Kela.Core.Modules.Auth.Entities.StudentDevice;

public sealed class StudentDeviceConfiguration : IEntityTypeConfiguration<StudentDevice>
{
    public void Configure(EntityTypeBuilder<StudentDevice> builder)
    {
        builder.ToTable("student_devices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.DeviceId).IsUnique();

        builder.HasOne<Modules.Auth.Entities.Student>().WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.StudentId);
    }
}
