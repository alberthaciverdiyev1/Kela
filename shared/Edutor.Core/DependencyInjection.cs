using FluentValidation;
using Edutor.Core.Common.Storage;
using Edutor.Core.Data;
using Edutor.Core.Modules.Banner.Services;
using Edutor.Core.Modules.Setting.Services;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Auth.Services;
using Edutor.Core.Modules.Subject.Services;
using Edutor.Core.Modules.Lesson.Services;
using Edutor.Core.Modules.LessonContent.Services;
using Edutor.Core.Modules.StudentApp.Services;
using Edutor.Core.Modules.Progress.Services;
using Edutor.Core.Modules.Question.Services;
using Edutor.Core.Modules.Quiz.Services;
using Edutor.Core.Modules.Notification.Services;
using Edutor.Core.Modules.PrivacyPolicy.Services;
using Edutor.Core.Modules.Tenant.Services;
using Edutor.Core.Modules.Dashboard.Services;
using Edutor.Core.Modules.Students.Services;
using Edutor.Core.Common.MultiTenancy;
using Edutor.Core.Common.Caching;
using Edutor.Core.Modules.Media.Services;
using Edutor.Core.Modules.SystemNotification.Services;
using Edutor.Core.Common.Email;
using Edutor.Core.Common.Push;
using Edutor.Core.Modules.User.Services;
using Edutor.Core.Modules.Group.Services;
using Edutor.Core.Modules.Attendance.Services;
using Edutor.Core.Modules.Nodes;
using Edutor.Core.Modules.Nodes.Abstractions;
using Edutor.Core.Modules.Nodes.Services;
using Edutor.Core.Modules.Subscription;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Edutor.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IAppCache, AppCache>();
        services.AddScoped<IUploadService, UploadService>();
        services.AddScoped<ISystemNotificationService, SystemNotificationService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Konfiqurasiya DİNLƏMƏ ZAMANI oxunur, qeydiyyat zamanı yox: host qurularkən
        // sonradan əlavə olunan mənbələr (testlər, env dəyişənləri) də nəzərə alınsın.
        services.AddDbContext<AppDbContext>((provider, options) =>
            options.UseNpgsql(provider.GetRequiredService<IConfiguration>().GetConnectionString("Postgres")));

        // Depolama sağlayıcısı: Storage:Provider = "Local" | "R2"
        services.AddSingleton<IFileStorage>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();

            var options = new R2Options();
            config.GetSection(R2Options.SectionName).Bind(options);

            var storageProvider = config["Storage:Provider"] ?? "Local";

            return string.Equals(storageProvider, "R2", StringComparison.OrdinalIgnoreCase)
                ? new R2FileStorage(Microsoft.Extensions.Options.Options.Create(options))
                : new LocalFileStorage(
                    provider.GetRequiredService<IWebHostEnvironment>(),
                    config,
                    provider.GetRequiredService<IHttpContextAccessor>());
        });

        services.Configure<WorkerOptions>(configuration.GetSection(WorkerOptions.SectionName));
        services.Configure<R2Options>(configuration.GetSection(R2Options.SectionName));

        services.AddScoped<IBannerService, BannerService>();
        services.AddScoped<Edutor.Core.Modules.Feedback.Abstractions.IFeedbackService, Edutor.Core.Modules.Feedback.Services.FeedbackService>();
        services.AddScoped<Edutor.Core.Modules.Certificate.Abstractions.ICertificateService, Edutor.Core.Modules.Certificate.Services.CertificateService>();
        services.AddScoped<ISettingService, SettingService>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IStudentDeviceService, StudentDeviceService>();
        services.AddScoped<IEmailSender, NoOpEmailSender>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<ILessonContentService, LessonContentService>();
        services.AddScoped<IStudentAccessService, StudentAccessService>();
        services.AddScoped<IStudentAppService, StudentAppService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IStudentQuizService, StudentQuizService>();
        services.AddScoped<IGroupQuizStatService, GroupQuizStatService>();
        services.AddScoped<Edutor.Core.Modules.ScreenGuard.Abstractions.IScreenGuardService, Edutor.Core.Modules.ScreenGuard.Services.ScreenGuardService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IHomeworkService, Edutor.Core.Modules.Homework.Services.HomeworkService>();
        services.AddScoped<IStudentHomeworkService, Edutor.Core.Modules.Homework.Services.StudentHomeworkService>();
        services.AddScoped<IPrivacyPolicyService, PrivacyPolicyService>();
        services.AddScoped<ITenantService, TenantService>();        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStudentManagementService, StudentManagementService>();
        services.AddScoped<Edutor.Core.Modules.Payments.Abstractions.IStudentPaymentService, Edutor.Core.Modules.Payments.Services.StudentPaymentService>();
        services.AddScoped<Edutor.Core.Modules.Payments.Abstractions.ICashboxService, Edutor.Core.Modules.Payments.Services.CashboxService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantScope, TenantScope>();

        // Arxa plan işləri (uzun sürən generasiya və s.)
        services.AddSingleton<Edutor.Core.Common.Jobs.BackgroundJobs>();
        services.AddSingleton<Edutor.Core.Common.Jobs.IBackgroundJobs>(sp => sp.GetRequiredService<Edutor.Core.Common.Jobs.BackgroundJobs>());
        services.AddHostedService<Edutor.Core.Common.Jobs.BackgroundJobWorker>();
        services.AddScoped<ITenantProvider, HttpTenantProvider>();

        services.Configure<FcmOptions>(configuration.GetSection(FcmOptions.SectionName));
        services.Configure<WebPushOptions>(configuration.GetSection(WebPushOptions.SectionName));
        // VAPID açarları yoxdursa push sükutla söndürülür (NoOp) — app yenə işləyir.
        services.AddSingleton<IPushSender>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<WebPushOptions>>();
            return options.Value.IsConfigured
                ? ActivatorUtilities.CreateInstance<WebPushSender>(sp)
                : ActivatorUtilities.CreateInstance<NoOpPushSender>(sp);
        });
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserAuthService, UserAuthService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ITeacherNoteService, Edutor.Core.Modules.TeacherNote.Services.TeacherNoteService>();
        services.AddScoped<ICalendarService, Edutor.Core.Modules.Calendar.Services.CalendarService>();
        services.AddScoped<ITenantConfigService, Edutor.Core.Modules.TenantConfig.Services.TenantConfigService>();
        services.AddScoped<IPaymentAccessService, Edutor.Core.Modules.Payments.Services.PaymentAccessService>();
        services.AddScoped<IVideoProcessingService, VideoProcessingService>();
        services.AddScoped<ITenantFolder, TenantFolder>();

        services.Configure<NodeOptions>(configuration.GetSection(NodeOptions.SectionName));
        services.Configure<SubscriptionOptions>(configuration.GetSection(SubscriptionOptions.SectionName));
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPlatformFinanceService, PlatformFinanceService>();
        services.AddScoped<IStorageMeteringService, StorageMeteringService>();
        services.AddScoped<INodeService, NodeService>();
        services.AddScoped<IVideoJobScheduler, VideoJobScheduler>();
        services.AddScoped<IVideoJobLifecycle, VideoJobLifecycle>();

        return services;
    }

    public static void UseCoreStorage(this IApplicationBuilder app)
    {
        var config = app.ApplicationServices.GetRequiredService<IConfiguration>();

        // R2 rejimində fayllar bucket-dan gəlir — lokal qovluq və /uploads mount lazımsızdır.
        var provider = config["Storage:Provider"] ?? "Local";
        if (!string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase)) return;

        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var root = StoragePaths.ResolveRoot(env, config);

        Directory.CreateDirectory(root);

        // Əks halda .m3u8/.ts brauzerdə tanınmır və yayım pozulur.
        var types = new FileExtensionContentTypeProvider();
        types.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
        types.Mappings[".ts"] = "video/mp2t";
        types.Mappings[".m4a"] = "audio/mp4";

        // Skript işlədə bilən fayllar heç vaxt inline açılmır — eyni origin-də
        // icra olunmaq admin sessiyasını oğurlamağa bərabərdir.
        var executable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".html", ".htm", ".xhtml", ".shtml", ".svg", ".svgz",
            ".xml", ".xsl", ".xslt", ".js", ".mjs", ".swf", ".wasm"
        };

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = StoragePaths.RequestPath,
            ContentTypeProvider = types,
            ServeUnknownFileTypes = false,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                var extension = Path.GetExtension(ctx.File.Name);
                if (!executable.Contains(extension)) return;

                ctx.Context.Response.ContentType = "application/octet-stream";
                var name = Uri.EscapeDataString(ctx.File.Name);
                ctx.Context.Response.Headers["Content-Disposition"] = $"attachment; filename*=UTF-8''{name}";
            }
        });
    }
}
