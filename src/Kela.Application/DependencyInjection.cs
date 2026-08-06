using FluentValidation;
using Kela.Application.Features.Sections;
using Kela.Application.Features.SiteConfiguration;
using Kela.Application.Features.Users;
using Kela.Application.Features.Users.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISiteConfigurationService, SiteConfigurationService>();

        // FluentValidation: AbstractValidator<T> türevlerini IValidator<T> olarak otomatik kaydeder.
        // Validatörler internal olduğundan includeInternalTypes gerekir.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
