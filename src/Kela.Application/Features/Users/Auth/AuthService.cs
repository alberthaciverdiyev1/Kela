using FluentValidation;
using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Responses;
using Kela.Application.Features.Users.Requests;
using Kela.Domain.Entities;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Users.Auth;

internal sealed class AuthService(
    UserManager<User> userManager,
    RoleManager<IdentityRole<int>> roleManager,
    SignInManager<User> signInManager,
    IValidator<LoginRequest> loginValidator,
    IValidator<RegisterRequest> registerValidator,
    IValidator<CreateUserRequest> createUserValidator) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

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
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await CreateUserCoreAsync(
            request.FirstName, request.LastName, request.Email, request.Password, Role.Teacher);

        return new RegisterResponse(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty);
    }

    public async Task<int> CreateUserAsync(
        CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await createUserValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await CreateUserCoreAsync(
            request.FirstName, request.LastName, request.Email, request.Password, request.Role);

        return user.Id;
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default) => signInManager.SignOutAsync();

    private async Task<User> CreateUserCoreAsync(
        string firstName, string lastName, string email, string password, Role role)
    {
        var trimmedEmail = email.Trim();
        var normalizedEmail = trimmedEmail.ToLowerInvariant();

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            throw new InvalidOperationException($"'{normalizedEmail}' email adresi zaten kayıtlı.");
        }

        var user = new User(firstName.Trim(), lastName.Trim(), trimmedEmail)
        {
            CreatedAt = DateTime.UtcNow,
        };

        // Identity: parolayı hash'ler (PBKDF2) ve kullanıcıyı kaydeder.
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        // Identity rol üyeliği (AspNetUserRoles).
        var roleName = role.ToString();
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }

        await userManager.AddToRoleAsync(user, roleName);

        return user;
    }
}
