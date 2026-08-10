using Kela.Application;
using Kela.Application.Features.Attendances;
using Kela.Application.Features.Cities;
using Kela.Application.Features.Contents;
using Kela.Application.Features.Nodes;
using Kela.Application.Features.Questions;
using Kela.Application.Features.Lessons;
using Kela.Application.Features.Quizzes;
using Kela.Application.Features.Students;
using Kela.Application.Features.SiteConfiguration;
using Kela.Application.Features.Users;
using Kela.Application.Features.Workspaces;
using Kela.Application.Patterns;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Kela.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kela.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("'ConnectionStrings:Postgres' yapılandırması bulunamadı.");

        // Npgsql veri kaynağı: Dictionary<string,string> gibi dinamik tiplerin jsonb
        // sütunlarıyla okunup yazılması için EnableDynamicJson opt-in edilir (Npgsql 8+).
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<KelaDbContext>(options =>
            options.UseNpgsql(dataSource));

        // ASP.NET Core Identity (tam entegrasyon):
        //   AddIdentity → UserManager, RoleManager, SignInManager + Identity cookie şeması
        //   (IdentityConstants.ApplicationScheme) + security stamp doğrulaması kurar.
        //   Parola hash'leme, lockout ve rol üyeliği buradan gelir — elle hash yoktur.
        services
            .AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Kurallar CreateUserValidator/UpdateUserValidator ile uyumlu (min 6, başka kısıt yok).
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<KelaDbContext>();

        // Display-name claim'ini "Ad Soyad" yapan özel claims factory (varsayılan: e-posta).
        services.AddScoped<IUserClaimsPrincipalFactory<User>, KelaUserClaimsPrincipalFactory>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ISiteConfigurationRepository, SiteConfigurationRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<INodeRepository, NodeRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<KelaDbContext>());

        return services;
    }
}
