using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.ScreenGuard.Configurations;

using ScreenViolation = Kela.Core.Modules.ScreenGuard.Entities.ScreenViolation;

public sealed class ScreenViolationConfiguration : IEntityTypeConfiguration<ScreenViolation>
{
    public void Configure(EntityTypeBuilder<ScreenViolation> builder)
    {
        builder.ToTable("screen_violations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Path).HasColumnName("path").HasMaxLength(500);
        builder.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne<Modules.Auth.Entities.Student>().WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
