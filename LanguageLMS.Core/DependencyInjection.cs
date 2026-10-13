using FluentValidation;
using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Banner.Services;
using LanguageLMS.Core.Modules.Setting.Services;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Auth.Services;
using LanguageLMS.Core.Modules.Subject.Services;
using LanguageLMS.Core.Modules.Lesson.Services;
using LanguageLMS.Core.Modules.LessonContent.Services;
using LanguageLMS.Core.Modules.StudentApp.Services;
using LanguageLMS.Core.Modules.Progress.Services;
using LanguageLMS.Core.Modules.Question.Services;
using LanguageLMS.Core.Modules.Quiz.Services;
using LanguageLMS.Core.Modules.Notification.Services;
using LanguageLMS.Core.Modules.PrivacyPolicy.Services;
using LanguageLMS.Core.Modules.Tenant.Services;
using LanguageLMS.Core.Modules.Permission.Services;
using LanguageLMS.Core.Modules.Dashboard.Services;
using LanguageLMS.Core.Modules.Students.Services;
using LanguageLMS.Core.Common.MultiTenancy;
using LanguageLMS.Core.Common.Caching;
using LanguageLMS.Core.Modules.Media.Services;
using LanguageLMS.Core.Modules.SystemNotification.Services;
using LanguageLMS.Core.Common.Email;
using LanguageLMS.Core.Common.Push;
using LanguageLMS.Core.Modules.User.Services;
using LanguageLMS.Core.Modules.Group.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace LanguageLMS.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IAppCache, AppCache>();
        services.AddScoped<IUploadService, UploadService>();
        services.AddScoped<ISystemNotificationService, SystemNotificationService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));


        // Depolama sağlayıcısı: Storage:Provider = "Local" | "R2"
        var storageProvider = configuration["Storage:Provider"] ?? "Local";
        if (string.Equals(storageProvider, "R2", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<R2Options>(configuration.GetSection(R2Options.SectionName));
            services.AddSingleton<IFileStorage, R2FileStorage>();
        }
        else
        {
            services.AddSingleton<IFileStorage, LocalFileStorage>();
        }

        services.AddScoped<IBannerService, BannerService>();
        services.AddScoped<LanguageLMS.Core.Modules.Certificate.Abstractions.ICertificateService, LanguageLMS.Core.Modules.Certificate.Services.CertificateService>();
        services.AddScoped<ISettingService, SettingService>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IStudentDeviceService, StudentDeviceService>();
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
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
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPrivacyPolicyService, PrivacyPolicyService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStudentManagementService, StudentManagementService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, HttpTenantProvider>();

        services.Configure<FcmOptions>(configuration.GetSection(FcmOptions.SectionName));
        services.AddSingleton<IPushSender, NoOpPushSender>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserAuthService, UserAuthService>();
        services.AddScoped<IGroupService, GroupService>();

        return services;
    }

    public static void UseCoreStorage(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var root = StoragePaths.ResolveRoot(env, config);

        Directory.CreateDirectory(root);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = StoragePaths.RequestPath
        });
    }
}
