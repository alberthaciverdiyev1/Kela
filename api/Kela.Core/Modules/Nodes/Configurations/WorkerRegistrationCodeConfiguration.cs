using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Nodes.Configurations;

using WorkerRegistrationCode = Kela.Core.Modules.Nodes.Entities.WorkerRegistrationCode;

public sealed class WorkerRegistrationCodeConfiguration : IEntityTypeConfiguration<WorkerRegistrationCode>
{
    public void Configure(EntityTypeBuilder<WorkerRegistrationCode> builder)
    {
        builder.ToTable("worker_registration_codes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(190);
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.UsedAt).HasColumnName("used_at");
        builder.Property(x => x.UsedByNodeId).HasColumnName("used_by_node_id");

        builder.HasIndex(x => x.Code).IsUnique().HasFilter("deleted_at IS NULL");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
