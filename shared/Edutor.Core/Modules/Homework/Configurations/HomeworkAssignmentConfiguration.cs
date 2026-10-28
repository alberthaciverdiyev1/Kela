using Edutor.Core.Modules.Homework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Homework.Configurations;

public sealed class HomeworkAssignmentConfiguration : IEntityTypeConfiguration<HomeworkAssignment>
{
    public void Configure(EntityTypeBuilder<HomeworkAssignment> builder)
    {
        builder.ToTable("homework_assignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.HomeworkId).HasColumnName("homework_id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.DueDate).HasColumnName("due_date");
        builder.Property(x => x.MaxScore).HasColumnName("max_score").HasPrecision(6, 2);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(16).IsRequired();
        builder.Property(x => x.Text).HasColumnName("text").HasMaxLength(10000);
        builder.Property(x => x.Link).HasColumnName("link").HasMaxLength(500);
        builder.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
        builder.Property(x => x.IsLate).HasColumnName("is_late").HasDefaultValue(false);
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count").HasDefaultValue(0);
        builder.Property(x => x.Score).HasColumnName("score").HasPrecision(6, 2);
        builder.Property(x => x.Feedback).HasColumnName("feedback").HasMaxLength(4000);
        builder.Property(x => x.GradedAt).HasColumnName("graded_at");
        builder.Property(x => x.GradedByUserId).HasColumnName("graded_by_user_id");
        builder.Property(x => x.ReturnedAt).HasColumnName("returned_at");

        builder.HasOne(x => x.Homework).WithMany(x => x.Assignments)
            .HasForeignKey(x => x.HomeworkId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Student).WithMany()
            .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.HomeworkId, x.StudentId }).IsUnique();
        builder.HasIndex(x => new { x.StudentId, x.Status });
        builder.HasIndex(x => x.DeletedAt);
    }
}

public sealed class HomeworkAssignmentFileConfiguration : IEntityTypeConfiguration<HomeworkAssignmentFile>
{
    public void Configure(EntityTypeBuilder<HomeworkAssignmentFile> builder)
    {
        builder.ToTable("homework_assignment_files");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.AssignmentId).HasColumnName("assignment_id");
        builder.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Path).HasColumnName("path").HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(128);
        builder.Property(x => x.SizeBytes).HasColumnName("size_bytes");

        builder.HasOne(x => x.Assignment).WithMany(x => x.Files)
            .HasForeignKey(x => x.AssignmentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.AssignmentId);
        builder.HasIndex(x => x.DeletedAt);
    }
}
