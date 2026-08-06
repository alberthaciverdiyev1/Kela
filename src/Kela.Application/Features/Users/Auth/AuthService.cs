using FluentValidation;
using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Responses;
using Kela.Application.Features.Users.Requests;
using Kela.Domain.Common;
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

        return new LoginResponse(user.Id, user.FirstName, user.LastName ?? string.Empty, role);
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await CreateUserCoreAsync(
            request.FirstName, request.LastName, request.Email, request.Password, RoleNames.Teacher);

        return new RegisterResponse(user.Id, user.FirstName, user.LastName ?? string.Empty, user.Email ?? string.Empty);
    }

    public async Task<int> CreateUserAsync(
        CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await createUserValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await CreateUserCoreAsync(
            request.FirstName, request.LastName, request.Email, request.Password, request.Role,
            request.PhoneNumber);

        return user.Id;
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default) => signInManager.SignOutAsync();

    private async Task<User> CreateUserCoreAsync(
        string firstName, string? lastName, string email, string password, string role, string? phoneNumber = null)
    {
        var trimmedEmail = email.Trim();
        var normalizedEmail = trimmedEmail.ToLowerInvariant();

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            throw new InvalidOperationException($"'{normalizedEmail}' email adresi zaten kayıtlı.");
        }

        var user = new User(firstName.Trim(), lastName, trimmedEmail)
        {
            CreatedAt = DateTime.UtcNow,
        };
        user.SetPhoneNumber(phoneNumber);

        // Identity: parolayı hash'ler (PBKDF2) ve kullanıcıyı kaydeder.
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        // Identity rol üyeliği (AspNetUserRoles).
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }

        await userManager.AddToRoleAsync(user, role);

        return user;
    }
}
