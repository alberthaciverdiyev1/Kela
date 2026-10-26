using Kela.Core.Modules.Homework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Homework.Configurations;

using Homework = Kela.Core.Modules.Homework.Entities.Homework;

public sealed class HomeworkConfiguration : IEntityTypeConfiguration<Homework>
{
    public void Configure(EntityTypeBuilder<Homework> builder)
    {
        builder.ToTable("homeworks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190).IsRequired();
        // Rich editor HTML saxlayır — mətn limitindən daha böyük sütun lazımdır.
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(20000);
        builder.Property(x => x.MaxScore).HasColumnName("max_score").HasPrecision(6, 2);
        builder.Property(x => x.DueDate).HasColumnName("due_date");
        builder.Property(x => x.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
        builder.Property(x => x.PublishedAt).HasColumnName("published_at");
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.IsPublished, x.DueDate });
        builder.HasIndex(x => x.DeletedAt);
    }
}

public sealed class HomeworkGroupConfiguration : IEntityTypeConfiguration<HomeworkGroup>
{
    public void Configure(EntityTypeBuilder<HomeworkGroup> builder)
    {
        builder.ToTable("homework_groups");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.HomeworkId).HasColumnName("homework_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");

        builder.HasOne(x => x.Homework).WithMany(x => x.Groups)
            .HasForeignKey(x => x.HomeworkId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Group).WithMany()
            .HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.HomeworkId, x.GroupId }).IsUnique();
        builder.HasIndex(x => x.DeletedAt);
    }
}

public sealed class HomeworkQuizConfiguration : IEntityTypeConfiguration<HomeworkQuiz>
{
    public void Configure(EntityTypeBuilder<HomeworkQuiz> builder)
    {
        builder.ToTable("homework_quizzes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.HomeworkId).HasColumnName("homework_id");
        builder.Property(x => x.QuizId).HasColumnName("quiz_id");
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");

        builder.HasOne(x => x.Homework).WithMany(x => x.Quizzes)
            .HasForeignKey(x => x.HomeworkId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Quiz).WithMany()
            .HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.HomeworkId, x.QuizId }).IsUnique();
        builder.HasIndex(x => x.DeletedAt);
    }
}
