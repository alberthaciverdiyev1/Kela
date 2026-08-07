using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).UseIdentityColumn().ValueGeneratedOnAdd();

        builder.Property(w => w.Name).HasMaxLength(100).IsRequired();

        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt);

        builder.HasOne(w => w.Teacher)
            .WithMany()
            .HasForeignKey(w => w.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Students)
            .WithMany()
            .UsingEntity(j => j.ToTable("workspace_students"));

        builder.HasIndex(w => new { w.TeacherId, w.Name }).IsUnique();
    }
}
