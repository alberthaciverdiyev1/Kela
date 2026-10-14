using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Permission.Configurations;

using Permission = Kela.Core.Modules.Permission.Entities.Permission;
using RolePermission = Kela.Core.Modules.Permission.Entities.RolePermission;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(30).IsRequired();
        builder.Property(x => x.PermissionId).HasColumnName("permission_id");

        builder.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.Role, x.PermissionId }).IsUnique();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}
