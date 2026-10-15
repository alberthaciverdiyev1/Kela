using System.Linq.Expressions;
using Kela.Core.Common.Entities;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Modules.Banner.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kela.Core.Data;

public sealed class AppDbContext : DbContext
{
    private static readonly Type[] TenantScopedPlainEntities =
    [
        typeof(Modules.Setting.Entities.Setting),
        typeof(Modules.PrivacyPolicy.Entities.PrivacyPolicy),
        typeof(Modules.Auth.Entities.OtpCode)
    ];

    private readonly bool _strictTenant;
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider, IConfiguration configuration)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _strictTenant = configuration.GetValue<bool>("Tenancy:Strict");
    }

    /// <summary>Cari tenant — canlı oxunur ki, login zamanı təyin olunan tenant dərhal tətbiq olunsun.</summary>
    public long? CurrentTenantId => _tenantProvider.TenantId;

    /// <summary>Superadmin cross-tenant rejimi — tenant filtri söndürülür.</summary>
    public bool IgnoreTenantFilter => _tenantProvider.IgnoreTenant;

    public DbSet<Banner> Banners => Set<Banner>();

    public DbSet<Modules.Certificate.Entities.Certificate> Certificates => Set<Modules.Certificate.Entities.Certificate>();

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

    public DbSet<Modules.PrivacyPolicy.Entities.PrivacyPolicy> PrivacyPolicies => Set<Modules.PrivacyPolicy.Entities.PrivacyPolicy>();

    public DbSet<Modules.Tenant.Entities.Tenant> Tenants => Set<Modules.Tenant.Entities.Tenant>();

    public DbSet<Modules.Auth.Entities.OtpCode> OtpCodes => Set<Modules.Auth.Entities.OtpCode>();

    public DbSet<Modules.Auth.Entities.StudentDevice> StudentDevices => Set<Modules.Auth.Entities.StudentDevice>();

    public DbSet<Modules.SystemNotification.Entities.SystemNotification> SystemNotifications => Set<Modules.SystemNotification.Entities.SystemNotification>();

    public DbSet<Modules.Group.Entities.MainCategoryAssignmentLog> MainCategoryAssignmentLogs => Set<Modules.Group.Entities.MainCategoryAssignmentLog>();

    public DbSet<Modules.ScreenGuard.Entities.ScreenViolation> ScreenViolations => Set<Modules.ScreenGuard.Entities.ScreenViolation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var isBase = typeof(BaseEntity).IsAssignableFrom(entityType.ClrType);
            var isTenantScopedPlain = TenantScopedPlainEntities.Contains(entityType.ClrType);

            if (!isBase && !isTenantScopedPlain) continue;

            entityType.FindProperty("TenantId")?.SetColumnName("tenant_id");

            // İki AYRI adlı filtri: "Tenant" (həmişə aktiv) və "SoftDelete" (yalnız BaseEntity).
            // Beləliklə silinmişləri göstərmək üçün yalnız SoftDelete söndürülə bilər — tenant filtri qalır.
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter("Tenant", BuildTenantFilter(entityType.ClrType));

            if (isBase)
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter("SoftDelete", BuildSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    /// <summary>Tenant filtri: cross-tenant (superadmin) → yoxdur; strict → yalnız cari tenant; əks halda tenant yoxdursa filtrsiz.</summary>
    private LambdaExpression BuildTenantFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");

        var ctx = Expression.Constant(this);
        var crossTenant = Expression.Property(ctx, nameof(IgnoreTenantFilter));
        var tenantValue = Expression.Property(ctx, nameof(CurrentTenantId));
        var tenantEquals = Expression.Equal(
            Expression.Convert(Expression.Property(parameter, "TenantId"), typeof(long?)),
            tenantValue);

        Expression tenantPart = _strictTenant
            ? tenantEquals
            : Expression.OrElse(Expression.Equal(tenantValue, Expression.Constant(null, typeof(long?))), tenantEquals);

        Expression body = Expression.OrElse(crossTenant, tenantPart);
        return Expression.Lambda(body, parameter);
    }

    /// <summary>Soft-delete filtri: deleted_at IS NULL.</summary>
    private static LambdaExpression BuildSoftDeleteFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");
        var body = Expression.Equal(
            Expression.Property(parameter, nameof(BaseEntity.DeletedAt)),
            Expression.Constant(null, typeof(DateTimeOffset?)));
        return Expression.Lambda(body, parameter);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    private void ApplyAudit()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            var hasTenant = entry.Metadata.FindProperty("TenantId") is not null;
            var isBaseEntity = entry.Entity is BaseEntity;

            if (!hasTenant && !isBaseEntity) continue;

            if (entry.State == EntityState.Added)
            {
                if (hasTenant && CurrentTenantId.HasValue)
                {
                    entry.Property("TenantId").CurrentValue = CurrentTenantId.Value;
                }

                if (isBaseEntity)
                {
                    ((BaseEntity)entry.Entity).CreatedAt = now;
                }
            }
            else if (entry.State == EntityState.Modified && isBaseEntity)
            {
                ((BaseEntity)entry.Entity).UpdatedAt = now;
            }
        }
    }
}
