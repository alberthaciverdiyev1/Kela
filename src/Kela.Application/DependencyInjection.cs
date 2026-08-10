using FluentValidation;
using Kela.Application.Features.Attendances;
using Kela.Application.Features.Cities;
using Kela.Application.Features.Contents;
using Kela.Application.Features.Lessons;
using Kela.Application.Features.Nodes;
using Kela.Application.Features.Questions;
using Kela.Application.Features.Quizzes;
using Kela.Application.Features.Students;
using Kela.Application.Features.SiteConfiguration;
using Kela.Application.Features.Users;
using Kela.Application.Features.Users.Auth;
using Kela.Application.Features.Workspaces;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISiteConfigurationService, SiteConfigurationService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<INodeService, NodeService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<ILessonService, LessonService>();

        // FluentValidation: AbstractValidator<T> türevlerini IValidator<T> olarak otomatik kaydeder.
        // Validatörler internal olduğundan includeInternalTypes gerekir.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
