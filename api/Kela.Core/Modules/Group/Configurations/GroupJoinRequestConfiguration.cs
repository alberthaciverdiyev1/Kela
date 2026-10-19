using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Group.Configurations;

using Group = Kela.Core.Modules.Group.Entities.Group;
using GroupJoinRequest = Kela.Core.Modules.Group.Entities.GroupJoinRequest;
using Student = Kela.Core.Modules.Auth.Entities.Student;

public sealed class GroupJoinRequestConfiguration : IEntityTypeConfiguration<GroupJoinRequest>
{
    public void Configure(EntityTypeBuilder<GroupJoinRequest> builder)
    {
        builder.ToTable("group_join_requests");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.DecidedAt).HasColumnName("decided_at");

        builder.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.GroupId, x.StudentId }).IsUnique();
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.DeletedAt);
    }
}
