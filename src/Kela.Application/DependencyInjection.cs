using Kela.Application.Features.Sections;
using Kela.Application.Features.Sections.Requests;
using Kela.Application.Features.Sections.Validators;
using Kela.Application.Features.Users;
using Kela.Application.Features.Users.Auth;
using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Validators;
using Kela.Application.Features.Users.Requests;
using Kela.Application.Features.Users.Validators;
using Kela.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        // Request doğrulayıcılar (service'ler çağırır, hatayı ValidationException ile fırlatır).
        services.AddScoped<IValidator<CreateSectionRequest>, CreateSectionValidator>();
        services.AddScoped<IValidator<UpdateSectionRequest>, UpdateSectionValidator>();
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginValidator>();

        return services;
    }
}
