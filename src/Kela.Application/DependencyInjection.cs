using Kela.Application.Sections;
using Kela.Application.Users;
using Kela.Application.Users.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
