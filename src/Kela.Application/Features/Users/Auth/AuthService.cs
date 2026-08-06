using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Responses;
using Kela.Application.Validation;
using Kela.Domain.Entities;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Users.Auth;

internal sealed class AuthService(
    UserManager<User> userManager,
    RoleManager<IdentityRole<int>> roleManager,
    SignInManager<User> signInManager,
    IValidator<LoginRequest> loginValidator,
    IValidator<RegisterRequest> registerValidator) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        loginValidator.Validate(request);

        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null || user.Status != UserStatus.Active)
        {
            return null;
        }

        var result = await signInManager.PasswordSignInAsync(
            user, request.Password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return null;
        }

        var role = await user.ResolveRoleAsync(userManager);

        return new LoginResponse(user.Id, user.FirstName, user.LastName, role);
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        registerValidator.Validate(request);

        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            throw new InvalidOperationException($"'{normalizedEmail}' email adresi zaten kayıtlı.");
        }

        var user = new User(request.FirstName.Trim(), request.LastName.Trim(), email)
        {
            CreatedAt = DateTime.UtcNow,
        };

        user.AssignProfile(Role.Teacher);

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var roleName = Role.Teacher.ToString();

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }

        await userManager.AddToRoleAsync(user, roleName);

        return new RegisterResponse(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty);
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default) => signInManager.SignOutAsync();
}
