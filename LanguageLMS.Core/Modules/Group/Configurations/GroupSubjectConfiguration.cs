using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.Group.Configurations;

using Group = LanguageLMS.Core.Modules.Group.Entities.Group;
using GroupSubject = LanguageLMS.Core.Modules.Group.Entities.GroupSubject;
using Subject = LanguageLMS.Core.Modules.Subject.Entities.Subject;

public sealed class GroupSubjectConfiguration : IEntityTypeConfiguration<GroupSubject>
{
    public void Configure(EntityTypeBuilder<GroupSubject> builder)
    {
        builder.ToTable("group_subjects");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.SubjectId).HasColumnName("subject_id");

        builder.HasOne(x => x.Group).WithMany(g => g.SubjectLinks).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => new { x.GroupId, x.SubjectId });
    }
}
