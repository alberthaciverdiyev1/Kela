using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class NodeConfiguration : IEntityTypeConfiguration<Node>
{
    public void Configure(EntityTypeBuilder<Node> builder)
    {
        builder.ToTable("nodes");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(n => n.Name).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Position).IsRequired();

        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt);

        builder.HasOne(n => n.Workspace)
            .WithMany(w => w.Nodes)
            .HasForeignKey(n => n.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Teacher)
            .WithMany()
            .HasForeignKey(n => n.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Parent)
            .WithMany(n => n.Children)
            .HasForeignKey(n => n.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Content)
            .WithMany(c => c.Nodes)
            .HasForeignKey(n => n.ContentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(n => n.DeletedAt == null);

        builder.HasIndex(n => new { n.WorkspaceId, n.ParentId, n.Position });
        builder.HasIndex(n => new { n.TeacherId, n.ParentId, n.Position });
    }
}
