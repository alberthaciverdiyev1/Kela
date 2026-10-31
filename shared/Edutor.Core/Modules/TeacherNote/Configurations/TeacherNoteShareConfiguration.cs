using Edutor.Core.Modules.TeacherNote.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.TeacherNote.Configurations;

using TeacherNoteShare = Edutor.Core.Modules.TeacherNote.Entities.TeacherNoteShare;

public sealed class TeacherNoteShareConfiguration : IEntityTypeConfiguration<TeacherNoteShare>
{
    public void Configure(EntityTypeBuilder<TeacherNoteShare> builder)
    {
        builder.ToTable("teacher_note_shares");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.TeacherNoteId).HasColumnName("teacher_note_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.TeacherNoteId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.DeletedAt);

        builder.HasOne<Entities.TeacherNote>()
            .WithMany(x => x.Shares)
            .HasForeignKey(x => x.TeacherNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Edutor.Core.Modules.User.Entities.User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
