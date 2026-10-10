using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.Group.Configurations;

using MainCategoryAssignmentLog = LanguageLMS.Core.Modules.Group.Entities.MainCategoryAssignmentLog;

public sealed class MainCategoryAssignmentLogConfiguration : IEntityTypeConfiguration<MainCategoryAssignmentLog>
{
    public void Configure(EntityTypeBuilder<MainCategoryAssignmentLog> builder)
    {
        builder.ToTable("main_category_assignment_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SubjectId).HasColumnName("subject_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.GroupId);
    }
}
