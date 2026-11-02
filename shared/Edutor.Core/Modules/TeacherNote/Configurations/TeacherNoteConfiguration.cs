using Edutor.Core.Modules.TeacherNote.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.TeacherNote.Configurations;

using TeacherNote = Edutor.Core.Modules.TeacherNote.Entities.TeacherNote;

public sealed class TeacherNoteConfiguration : IEntityTypeConfiguration<TeacherNote>
{
    public void Configure(EntityTypeBuilder<TeacherNote> builder)
    {
        builder.ToTable("teacher_notes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Body).HasColumnName("body").HasMaxLength(20000);
        builder.Property(x => x.Date).HasColumnName("date");
        builder.Property(x => x.IsDone).HasColumnName("is_done").HasDefaultValue(false);
        builder.Property(x => x.Color).HasColumnName("color").HasMaxLength(20).IsRequired().HasDefaultValue("default");
        builder.Property(x => x.IsPinned).HasColumnName("is_pinned").HasDefaultValue(false);
        builder.Property(x => x.ContentId).HasColumnName("content_id");
        builder.Property(x => x.PositionSeconds).HasColumnName("position_seconds");
        builder.Property(x => x.Visibility).HasColumnName("visibility").HasMaxLength(16).IsRequired()
            .HasDefaultValue(TeacherNoteVisibility.Private);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => new { x.UserId, x.Date });
        builder.HasIndex(x => x.ContentId);

        builder.HasOne<Edutor.Core.Modules.LessonContent.Entities.LessonContent>()
            .WithMany()
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
