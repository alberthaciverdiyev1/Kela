using System.Linq.Expressions;
using LanguageLMS.Core.Common.Entities;
using LanguageLMS.Core.Modules.Banner.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Banner> Banners => Set<Banner>();

    public DbSet<Modules.Setting.Entities.Setting> Settings => Set<Modules.Setting.Entities.Setting>();

    public DbSet<Modules.Auth.Entities.Student> Students => Set<Modules.Auth.Entities.Student>();

    public DbSet<Modules.Subject.Entities.Subject> Subjects => Set<Modules.Subject.Entities.Subject>();

    public DbSet<Modules.Lesson.Entities.Lesson> Lessons => Set<Modules.Lesson.Entities.Lesson>();

    public DbSet<Modules.Media.Entities.Video> Videos => Set<Modules.Media.Entities.Video>();

    public DbSet<Modules.Media.Entities.MediaFile> MediaFiles => Set<Modules.Media.Entities.MediaFile>();

    public DbSet<Modules.Media.Entities.VoiceRecord> VoiceRecords => Set<Modules.Media.Entities.VoiceRecord>();

    public DbSet<Modules.LessonContent.Entities.LessonContent> LessonContents => Set<Modules.LessonContent.Entities.LessonContent>();

    public DbSet<Modules.Progress.Entities.LessonProgress> LessonProgress => Set<Modules.Progress.Entities.LessonProgress>();

    public DbSet<Modules.Question.Entities.Question> Questions => Set<Modules.Question.Entities.Question>();

    public DbSet<Modules.Quiz.Entities.Quiz> Quizzes => Set<Modules.Quiz.Entities.Quiz>();

    public DbSet<Modules.Quiz.Entities.QuizQuestion> QuizQuestions => Set<Modules.Quiz.Entities.QuizQuestion>();

    public DbSet<Modules.Quiz.Entities.StudentQuiz> StudentQuizzes => Set<Modules.Quiz.Entities.StudentQuiz>();

    public DbSet<Modules.Quiz.Entities.StudentQuizQuestion> StudentQuizQuestions => Set<Modules.Quiz.Entities.StudentQuizQuestion>();

    public DbSet<Modules.User.Entities.User> Users => Set<Modules.User.Entities.User>();

    public DbSet<Modules.Group.Entities.Group> Groups => Set<Modules.Group.Entities.Group>();

    public DbSet<Modules.Group.Entities.StudentGroup> StudentGroups => Set<Modules.Group.Entities.StudentGroup>();

    public DbSet<Modules.Group.Entities.GroupSubject> GroupSubjects => Set<Modules.Group.Entities.GroupSubject>();

    public DbSet<Modules.Group.Entities.GroupQuiz> GroupQuizzes => Set<Modules.Group.Entities.GroupQuiz>();

    public DbSet<Modules.Notification.Entities.Notification> Notifications => Set<Modules.Notification.Entities.Notification>();

    public DbSet<Modules.Notification.Entities.NotificationStudent> NotificationStudents => Set<Modules.Notification.Entities.NotificationStudent>();

    public DbSet<Modules.Notification.Entities.NotificationToken> NotificationTokens => Set<Modules.Notification.Entities.NotificationToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var deletedAt = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
            var filter = Expression.Lambda(
                Expression.Equal(deletedAt, Expression.Constant(null, typeof(DateTimeOffset?))),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    private void ApplyTimestamps()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}
