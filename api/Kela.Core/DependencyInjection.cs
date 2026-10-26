using FluentValidation;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Banner.Services;
using Kela.Core.Modules.Setting.Services;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Auth.Services;
using Kela.Core.Modules.Subject.Services;
using Kela.Core.Modules.Lesson.Services;
using Kela.Core.Modules.LessonContent.Services;
using Kela.Core.Modules.StudentApp.Services;
using Kela.Core.Modules.Progress.Services;
using Kela.Core.Modules.Question.Services;
using Kela.Core.Modules.Quiz.Services;
using Kela.Core.Modules.Notification.Services;
using Kela.Core.Modules.PrivacyPolicy.Services;
using Kela.Core.Modules.Tenant.Services;
using Kela.Core.Modules.Dashboard.Services;
using Kela.Core.Modules.Students.Services;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Common.Caching;
using Kela.Core.Modules.Media.Services;
using Kela.Core.Modules.SystemNotification.Services;
using Kela.Core.Common.Email;
using Kela.Core.Common.Push;
using Kela.Core.Modules.User.Services;
using Kela.Core.Modules.Group.Services;
using Kela.Core.Modules.Attendance.Services;
using Kela.Core.Modules.Nodes;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Core.Modules.Nodes.Services;
using Kela.Core.Modules.Subscription;
using Kela.Core.Modules.Subscription.Abstractions;
using Kela.Core.Modules.Subscription.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Kela.Core;

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
        services.AddScoped<Kela.Core.Modules.Feedback.Abstractions.IFeedbackService, Kela.Core.Modules.Feedback.Services.FeedbackService>();
        services.AddScoped<Kela.Core.Modules.Certificate.Abstractions.ICertificateService, Kela.Core.Modules.Certificate.Services.CertificateService>();
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
        services.AddScoped<Kela.Core.Modules.ScreenGuard.Abstractions.IScreenGuardService, Kela.Core.Modules.ScreenGuard.Services.ScreenGuardService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IHomeworkService, Kela.Core.Modules.Homework.Services.HomeworkService>();
        services.AddScoped<IStudentHomeworkService, Kela.Core.Modules.Homework.Services.StudentHomeworkService>();
        services.AddScoped<IPrivacyPolicyService, PrivacyPolicyService>();
        services.AddScoped<ITenantService, TenantService>();        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStudentManagementService, StudentManagementService>();
        services.AddScoped<Kela.Core.Modules.Payments.Abstractions.IStudentPaymentService, Kela.Core.Modules.Payments.Services.StudentPaymentService>();
        services.AddScoped<Kela.Core.Modules.Payments.Abstractions.IKassaService, Kela.Core.Modules.Payments.Services.KassaService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantScope, TenantScope>();

        // Arxa plan işləri (uzun sürən generasiya və s.)
        services.AddSingleton<Kela.Core.Common.Jobs.BackgroundJobs>();
        services.AddSingleton<Kela.Core.Common.Jobs.IBackgroundJobs>(sp => sp.GetRequiredService<Kela.Core.Common.Jobs.BackgroundJobs>());
        services.AddHostedService<Kela.Core.Common.Jobs.BackgroundJobWorker>();
        services.AddScoped<ITenantProvider, HttpTenantProvider>();

        services.Configure<FcmOptions>(configuration.GetSection(FcmOptions.SectionName));
        services.AddSingleton<IPushSender, NoOpPushSender>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserAuthService, UserAuthService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IVideoProcessingService, VideoProcessingService>();
        services.AddScoped<ITenantFolder, TenantFolder>();

        services.Configure<NodeOptions>(configuration.GetSection(NodeOptions.SectionName));
        services.Configure<SubscriptionOptions>(configuration.GetSection(SubscriptionOptions.SectionName));
        services.AddScoped<ISubscriptionService, SubscriptionService>();
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
